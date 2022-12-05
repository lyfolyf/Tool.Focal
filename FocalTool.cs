using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using FocalSpec.FsApiNet.Model;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Globalization;
using CommonStruct;
using CommonStruct.Type3D;
using System.Diagnostics;
using Lead.Tool.XML;
using Lead.Tool.Interface;
using Lead.Tool.CommonData_3D;
using Lead.Tool.Log;

namespace Lead.Tool.Focal
{
    public enum ExportLayer
    {
        All,
        Top,
        Bottom,
        Brightest
    }

    public class FocalTool : ITool
    {
        public Config _Config;
        private ConfigUI _ConfigControl = null;
        private DebugUI _DebugControl = null;
        private IToolState _State = IToolState.ToolMin;
        public string _ConfigPath = "";
        private List<FSPoint[]> _ScanResult = new List<FSPoint[]>();
        private bool IsDataRecivedEnable = false;

        private FsApi.Header _header = new FsApi.Header();
        private static List<string> FocalList = new List<string>();

        private bool _dataRequest = false;

        private static FsApi _fsCore = null;
        private static bool IsCLosed = false;
       public float XStartPos { get; set; }
        public float YStartPos { get; set; }
        public float ZStartPos { get; set; }


        int _FirstLocation = -1;
        int _CurrentLocation = -1;
        uint _FirstIndex = 0;
        uint _CurrentIndex = 0;

        float _inervalTemp = 0;
        private FocalTool()
        {

        }

        public FocalTool(string Name, string Path)
        {
            _ConfigPath = Path;
            if (File.Exists(Path))
            {
                _Config = (Config)XmlSerializerHelper.ReadXML(Path, typeof(Config));
            }
            else
            {
                _Config = new Config();
            }

            _ConfigControl = new ConfigUI(this);
            _DebugControl = new DebugUI(this);

            _Config.Name = Name;
        }

        public List<FSPoint[]> GetScanResult()
        {
            //if (_ScanResult.Count > 0)
            //{
            //    Logger.Warn(_Config.Name + " _ScanResult.count = " + _ScanResult.Count);
            //}

            return _ScanResult;
        }

        public void ClearScanResult()
        {
            _FirstLocation = -1;
            _FirstIndex = 0;
            _ScanResult.Clear();
            Logger.Info(_Config.Name + " 清除缓存数据成功！");
        }

        public Control ConfigUI
        {
            get
            {
                return _ConfigControl;
            }
        }

        public Control DebugUI
        {
            get
            {
                return _DebugControl;
            }
        }

        public IToolState State
        {
            get
            {
                return _State;
            }
        }

        List<FSPoint[]> _Points = new List<FSPoint[]>() ;
        int StartIndex = 0;
        private FSPoint[] GetPoints(float[] zValues, float[] intensityValues, int lineLength, double xStep, FsApi.Header header, int Count)
        {
            FocalConfig _fsCoreConfig = _Config.ForeConfig;

            //List<FSPoint> Points = new List<FSPoint>();

            if (_Points[StartIndex].Length != lineLength)
            {
                _Points[StartIndex] = new FSPoint[lineLength];
                for (int i = 0; i < lineLength; i++)
                {
                    _Points[StartIndex][i] = new FSPoint();
                }
            }
            
            float no_meas = FsApi.NoMeasurement - 1;
            //for (int i = 0; i < lineLength; i = i + 3)
            for (int i = 0; i < lineLength; i++)
            {

                if (zValues[i] > no_meas)
                {
                    zValues[i] = 999998;
                    //continue;
                }
                //FSPoint temp = new FSPoint();

                if (_fsCoreConfig.IsXAxisTrige)
                {
                    _Points[StartIndex][i].X = -(XStartPos + Count * _inervalTemp);
                    //should be changed by TriggerTable
                    _Points[StartIndex][i].Y = -YStartPos + i * xStep * _fsCoreConfig.ProfileScale;
                }
                else
                {
                    _Points[StartIndex][i].Y = -(XStartPos + Count * _inervalTemp);
                    _Points[StartIndex][i].X = -YStartPos + i * xStep * _fsCoreConfig.ProfileScale;

                    //if (_fsCoreConfig.FlipXEnabled == 0)
                    //{
                    //    //正常
                    //    temp.Y = -(XStartPos + Count * _inervalTemp);
                    //    temp.X = -YStartPos + i * xStep * _fsCoreConfig.ProfileScale;
                    //}
                    //else if (_fsCoreConfig.FlipXEnabled == 1)
                    //{
                    //    //正常
                    //    temp.Y = -(XStartPos + Count * _inervalTemp);
                    //    temp.X = -YStartPos + (1706 - i) * xStep * _fsCoreConfig.ProfileScale;
                    //}

                    //temp.X = XStartPos + i * xStep * FSCoreDefines.ProfileScale;
                    //temp.Y = YStartPos + _listBatch.Count * _inervalTemp;
                }

                _Points[StartIndex][i].Z = zValues[i] * _fsCoreConfig.ProfileScale;
                _Points[StartIndex][i].Intensity = (int)intensityValues[i];
                _Points[StartIndex][i].Location = header.Location;
                _Points[StartIndex][i].index = header.Index;

                //Points.Add(temp);
                //return Points;
            }
            StartIndex++;
            return _Points[StartIndex-1];
        }

        private void LineCallback(int layerId, float[] zValues, float[] intensityValues, int lineLength, double xStep, FsApi.Header headerIn)
        {
            if (!IsDataRecivedEnable)
            {
                return;
            }

            FocalConfig _fsCoreConfig = _Config.ForeConfig;

            if (layerId > 0) 
                return;

            if (!_dataRequest) return;
            //Logger.Info(_Config.Name + ":" + _ScanResult.Count);

            _inervalTemp = _fsCoreConfig.TrigInterval;
            if (!_fsCoreConfig.IsExternalPulsingEnabled)
            {
                _inervalTemp = 1.0f / (float)_fsCoreConfig.Freq;
            }

            var _NowFre = headerIn.ReceptionFrequency;//接收频率

            int curLocation = headerIn.Location;
            //   if (_FirstLocation==-1) _FirstLocation = headerIn.Location;
            if (_FirstLocation == -1)
            {
                _FirstIndex = headerIn.Index;
                _FirstLocation = curLocation;
            }
            _CurrentIndex = headerIn.Index;
            _CurrentLocation = headerIn.Location;


            FSPoint[] processed = GetPoints(zValues, intensityValues, lineLength, xStep, headerIn, headerIn.Location - _FirstLocation);

            _ScanResult.Add(processed);
        }

        private int SetCallBackFuncByExportLayer(string camId, ExportLayer layer)
        {
            int iRet = 0;

            if (_fsCore == null) { return iRet; }

            switch (layer)
            {
                case ExportLayer.All:
                    _fsCore.StopGrabbing(camId);
                    // this._fsCore.RemoveLineCallback(this._cameraId);
                    _fsCore.RemoveLineCallback(camId);
                    _fsCore.SetProfileCallback(camId, ProfileReceptionCallback);
                    _fsCore.StartGrabbing(camId);
                    break;


                case ExportLayer.Top:
                    _fsCore.StopGrabbing(camId);
                    _fsCore.RemoveProfileCallback(camId);
                    _fsCore.SetLineSortingOrder(camId, SortingOrder.FromTopToBottom);
                    _fsCore.SetLineCallback(camId, 0, this.LineCallback);
                    _fsCore.StartGrabbing(camId);
                    break;

                case ExportLayer.Bottom:
                    _fsCore.StopGrabbing(camId);
                    _fsCore.RemoveProfileCallback(camId);
                    _fsCore.SetLineSortingOrder(camId, SortingOrder.FromBottomToTop);
                    _fsCore.SetLineCallback(camId, 0, this.LineCallback);
                    _fsCore.StartGrabbing(camId);
                    break;

                case ExportLayer.Brightest:
                    _fsCore.StopGrabbing(camId);
                    _fsCore.RemoveProfileCallback(camId);
                    _fsCore.SetLineSortingOrder(camId, SortingOrder.FromMaxIntensityToLower);
                    _fsCore.SetLineCallback(camId, 0, this.LineCallback);
                    _fsCore.StartGrabbing(camId);
                    break;
                default:
                    break;
            }

            return iRet;
        }

        //初始化并连接相机
        private CameraStatusCode InitAndConnectCamera(string camId) 
        {
            FocalConfig _fsCoreConfig = _Config.ForeConfig;
            _header.ReceptionQueueSize = 0;

            CameraStatusCode cameraStatus;

            var _cameraId = camId;

            cameraStatus = _fsCore.Connect(_cameraId,"");

            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;

            int calibrationsInCamera;
            cameraStatus = _fsCore.GetParameter(_cameraId, SensorParameter.SensorDataInFlash, out calibrationsInCamera);
            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;

            cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.MaxPointCount, 20000);
            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;

            if (calibrationsInCamera == 1)//==1，focal 机台本身带有校准文件，不需要倒入X，Z校准文件
            {
                _fsCoreConfig.ZCalibrationFile = null;
                cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.ZCalibrationFile, _fsCoreConfig.ZCalibrationFile);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;

                _fsCoreConfig.XCalibrationFile = null;
                cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.XCalibrationFile, _fsCoreConfig.XCalibrationFile);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;
            }
            else if (_fsCoreConfig.ZCalibrationFile != null || _fsCoreConfig.XCalibrationFile != null)
            {
                var zCalibStatus = _fsCore.SetParameter(_cameraId, SensorParameter.ZCalibrationFile, _fsCoreConfig.ZCalibrationFile);

                if (zCalibStatus == CameraStatusCode.CameraErrorCalibFileAttributeNotFound ||
                    zCalibStatus == CameraStatusCode.CameraErrorCalibFileAttributeInvalid ||
                    zCalibStatus == CameraStatusCode.CameraErrorInvalidCalibrationFile)
                {
                    // clear if not valid calibration.
                    _fsCoreConfig.ZCalibrationFile = null;
                }

                var xCalibStatus = _fsCore.SetParameter(_cameraId, SensorParameter.XCalibrationFile, _fsCoreConfig.XCalibrationFile);

                if (xCalibStatus == CameraStatusCode.CameraErrorCalibFileAttributeNotFound ||
                    xCalibStatus == CameraStatusCode.CameraErrorCalibFileAttributeInvalid ||
                    xCalibStatus == CameraStatusCode.CameraErrorInvalidCalibrationFile)
                {
                    // clear if not valid calibration.
                    _fsCoreConfig.XCalibrationFile = null;
                }

                if (zCalibStatus != CameraStatusCode.Ok)
                    return zCalibStatus;

                if (xCalibStatus != CameraStatusCode.Ok)
                    return xCalibStatus;
            }

            cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.PeakYUnit, 1);
            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;

            if (_fsCoreConfig.ZCalibrationFile == null)
            {   // read z calibration path from API and fill it to the settings 

                string zcalibrationFile;
                cameraStatus = _fsCore.GetParameter(_cameraId, SensorParameter.ZCalibrationFile, out zcalibrationFile);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;

                _fsCoreConfig.ZCalibrationFile = zcalibrationFile;
            }

            cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.PeakXUnit, 1);// 0: unit: pixel; 1: unit: micron
            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;

            if (_fsCoreConfig.XCalibrationFile == null)
            {   // read x calibration path from API and fill it to the settings 
                string xcalibrationFile;
                cameraStatus = _fsCore.GetParameter(_cameraId, SensorParameter.XCalibrationFile, out xcalibrationFile);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;

                _fsCoreConfig.XCalibrationFile = xcalibrationFile;
            }
            cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.PeakThreshold, 20);
            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;

            cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.FirLength, 16);
            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;
            int currentSensor;
            cameraStatus = _fsCore.GetParameter(_cameraId, SensorParameter.SensorType, out currentSensor);
            _fsCoreConfig.SensorType = currentSensor;
            if (cameraStatus != CameraStatusCode.Ok)
                return cameraStatus;

            if (currentSensor <= 1600)
            {
                cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.Gain, 1.0);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;

                cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.Mtu, 9014);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;

                // Set Interpacket Delay, if needed. By default, it's 20.
                cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.Ifg, 20);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;
            }

            if (currentSensor > 1600)
            {
                cameraStatus = _fsCore.SetParameter(_cameraId, SensorParameter.Gain, _Config.ForeConfig.Gain);
                if (cameraStatus != CameraStatusCode.Ok)
                    return cameraStatus;
            }

            return cameraStatus;
        }

        private void ProfileReceptionCallback(IList<FsApi.Point> profile, FsApi.Header headerIn)
        {

        }

        public void StartMeasure()
        {
            IsDataRecivedEnable = true;
            StartIndex = 0;

            //FocalConfig _fsCoreConfig = _Config.ForeConfig;
            //CameraStatusCode status;
            //var _cameraid = _Config.ForeConfig.CameraId;
            //status = _fsCore.StartGrabbing(_cameraid);

            //if (status != CameraStatusCode.Ok)
            //{
            //    Logger.Error(_Config.Name + " 开始采集失败！");
            //    throw new Exception(_Config.Name + " 开始采集失败！");
            //}
            //else
            //{
            //    Logger.Info(_Config.Name + " 开始采集成功！");
            //}
        }

        public void StopMeasure()
        {
            IsDataRecivedEnable = false;

            //FocalConfig _fsCoreConfig = _Config.ForeConfig;
            //CameraStatusCode status;
            //var _cameraid = _Config.ForeConfig.CameraId;
            //status = _fsCore.StopGrabbing(_cameraid);
            //if (status != CameraStatusCode.Ok)
            //{
            //    Logger.Error(_Config.Name + " 停止采集失败！");
            //    throw new Exception(_Config.Name + " 停止采集失败！");
            //}
            //else
            //{
            //    Logger.Info("_CurrentIndex = " + _CurrentIndex + "; _FirstIndex = " + _FirstIndex);
            //    Logger.Info("_CurrentLocation = " + _CurrentLocation+ "; _FirstLocation = "+ _FirstLocation);
            //    Logger.Info(_Config.Name + " 停止采集成功！");
            //}
            Logger.Info("_CurrentIndex = " + _CurrentIndex + "; _FirstIndex = " + _FirstIndex);
            Logger.Info("_CurrentLocation = " + _CurrentLocation + "; _FirstLocation = " + _FirstLocation);
            Logger.Info("触发点位数"+(_CurrentLocation - _FirstLocation)/2);
            Logger.Info("触发完成点位数" + (_CurrentIndex - _FirstIndex));
            Logger.Info(_Config.Name + " 停止采集成功！");
        }

        public void Init()
        {
            Logger.Info(_Config.Name + "初始化开始");

            int cameraCount = 3;
            List<string> cameraIds;
            int SensorDiscoveryTimeout = 2000;
           
            ExportLayer Layer = _Config.ForeConfig.Mode;
            IsDataRecivedEnable = false;

            try
            {
                for(int i = _Points.Count; i < 30000; i++)
                {
                    _Points.Add(new FSPoint[1500]);
                    for (int j = 0; j < _Points[i].Length; j++)
                    {
                        _Points[i][j] = new FSPoint();
                    }
                }

                if (null == _fsCore)
                {
                    _fsCore = new FsApi();
                    Logger.Info("new FsApi 成功");
                }

                if (FocalList.Count == 0)
                {
                    var cameraStatus = _fsCore.Open(ref cameraCount, out cameraIds, SensorDiscoveryTimeout);
                    if (cameraStatus != CameraStatusCode.Ok)
                    {
                        throw new Exception("Open 错误：" + cameraStatus.ToString());
                    }
                    Logger.Info("_fsCore.Open 成功");

                    if (cameraCount == 0)
                    {
                        _fsCore.Close();
                        //未连接成功
                        throw new Exception("未连接成功：cameraCount=0");
                    }

                    foreach (var camId in cameraIds)
                    {
                        FocalList.Add(camId);
                    }

                    Logger.Info("FocalList的数目不为0，证明以及open过Focal,本次跳过open");
                }


                var re = InitAndConnectCamera(_Config.ForeConfig.CameraId);
                if (re != CameraStatusCode.Ok)
                {
                    throw new Exception(_Config.Name + "InitAndConnectCamera 错误：" + re.ToString());
                }
                Logger.Info("_fsCore InitAndConnectCamera 成功");

                SetCallBackFuncByExportLayer(_Config.ForeConfig.CameraId, Layer);
                Logger.Info("_fsCore SetCallBackFuncByExportLayer 成功");

                _State = Interface.IToolState.ToolInit;

                Logger.Info(_Config.Name + "初始化成功！");
            }
            catch (Exception ex)
            {

                Logger.Error(_Config.Name + " 初始化失败：" + ex.Message);
                throw new Exception(_Config.Name + " 初始化失败：" + ex.Message);
            }
        }

        private void SetParam(string Name,int value)
        {
            var status = _fsCore.SetParameter(_Config.ForeConfig.CameraId, Name, value);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.ForeConfig.CameraId + " : " + Name + "设置出错:"+status.ToString());
            }

        }
        private void SetParam(string Name, double value)
        {
            var status = _fsCore.SetParameter(_Config.ForeConfig.CameraId, Name, value);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.ForeConfig.CameraId + " : " + Name + "设置出错:" + status.ToString());
            }
        }
        private void SetParam(string Name, float value)
        {
            var status = _fsCore.SetParameter(_Config.ForeConfig.CameraId, Name, value);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.ForeConfig.CameraId + " : " + Name + "设置出错:" + status.ToString());
            }
        }

        public void Start()
        {
            Logger.Info(_Config.Name + "启动开始");

            FocalConfig _fsCoreConfig = _Config.ForeConfig;
            CameraStatusCode status;
            var _cameraId  = _Config.ForeConfig.CameraId;

            //0
            status = _fsCore.StopGrabbing(_cameraId);
            //1
            int outValue = 0;
            if (_fsCoreConfig.AutoHeightZeroPosition)
            {
                status = _fsCore.AdjustRoiAndFps(_cameraId, 0, _fsCoreConfig.Freq, out outValue);//0表示有效景深在CMOS上居中
            }
            else
            {
                status = _fsCore.AdjustRoiAndFps(_cameraId, 6, _fsCoreConfig.Freq, out outValue);//6表示有效景深关于Z=0对称分布
            }

            SetParam(SensorParameter.PulseWidth, _fsCoreConfig.LedPulseWidth);
            status = _fsCore.SetParameter(_cameraId, SensorParameter.PulseFrequency, _fsCoreConfig.IsExternalPulsingEnabled ? 0 : _fsCoreConfig.Freq);


            //2
            status = _fsCore.StopGrabbing(_cameraId);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.ForeConfig.CameraId + " StopGrabbing 出错:" + status.ToString());
            }
            Thread.Sleep(1000);

            int? maxUs = 3000;
            bool IsAgcSupported = false;
            if (maxUs.HasValue && IsAgcSupported)
            {
                if (_fsCoreConfig.LedPulseWidth > maxUs.Value)
                    throw new ArgumentException("LED pulse start reference must be smaller than LED pulse upper limit (max).");

                status = _fsCore.SetParameter(_cameraId, SensorParameter.AgcWiLimit, maxUs.Value);

                if (status != CameraStatusCode.Ok)
                {
                    status = _fsCore.StartGrabbing(_cameraId);
                    Thread.Sleep(20);
                }
            }


            status = _fsCore.StartGrabbing(_cameraId);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.ForeConfig.CameraId + " StopGrabbing 出错:" + status.ToString());
            }
            Thread.Sleep(1000);

            //3
            SetParam( SensorParameter.PeakThreshold, _fsCoreConfig.PeakThreshold);
            SetParam( SensorParameter.FirLength, _fsCoreConfig.FirLength);
            SetParam(SensorParameter.SignalDetectionFilterLength, _fsCoreConfig.SignalDetectionFilterLength);
            SetParam(SensorParameter.AverageIntensityFilterSize, _fsCoreConfig.AverageIntensityFilterSize);

            SetParam(SensorParameter.PeakXFilter, _fsCoreConfig.PeakXFilter);
            SetParam(SensorParameter.NoiseRemoval, _fsCoreConfig.NoiseRemoval);

            //20200730
            SetParam(SensorParameter.FillGapXmax, _fsCoreConfig.FillGapXmax);
            SetParam(SensorParameter.MedianZFilterSize, _fsCoreConfig.MedianZFilterSize);
            SetParam(SensorParameter.ClusterMinimumLength, _fsCoreConfig.ClusterMinimumLength);

            //6
            //SetParam( SensorParameter.FirAverLength, _fsCoreConfig.AverFirLength);

            //7
            SetParam( SensorParameter.PulseDivider, _fsCoreConfig.RegPulseDivider);

            //8
            //SetParam( SensorParameter.Reordering, _fsCoreConfig.Reordering);

            //9
            //SetParam( SensorParameter.DetectMissingFirstLayer, _fsCoreConfig.DetectMissingFirstLayer);

            //10
            //SetParam( SensorParameter.FillGapXmax, _fsCoreConfig.FillGapXMax);

            //11
            //SetParam( SensorParameter.AverageZFilterSize, _fsCoreConfig.AverageZFilterSize);

            //12
            //13
            //SetParam( SensorParameter.MedianZFilterSize, _fsCoreConfig.MedianZFilterSize);

            //14
            //SetParam( SensorParameter.MedianIntensityFilterSize, _fsCoreConfig.MedianIntensityFilterSize);

            //15
            //SetParam( SensorParameter.ResampleLineXResolution, _fsCoreConfig.ResampleLineXResolution);

            //16

            //17
            //SetParam( SensorParameter.Heartbeat, 3000);

            //18
            status = _fsCore.StartGrabbing(_cameraId);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.Name + " "+_Config.ForeConfig.CameraId + " StopGrabbing 出错:" + status.ToString());
            }

            _State = Interface.IToolState.ToolRunning;

            ClearScanResult();

            _dataRequest = true;

            Logger.Info(_Config.Name + "启动成功");
        }

        public void Terminate()
        {
            Logger.Info(_Config.Name + "终止开始");

            if (IsCLosed == false)
            {
                _fsCore.Close();
                IsCLosed = true;
            }

            _State = Interface.IToolState.ToolTerminate;
            Logger.Info(_Config.Name + "终止成功");
        }

        public void SetDiffProdutionParam(FocalConfigDiffProdution Value)
        {
            this.StopMeasure();
            var _cameraId = _Config.ForeConfig.CameraId;

            var status = _fsCore.StopGrabbing(_cameraId);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_cameraId + " StopGrabbing 出错:" + status.ToString());
            }
            Thread.Sleep(100);

            SetParam(SensorParameter.FillGapXmax, Value.FillGapXmax);

            int currentSensor;
            status = _fsCore.GetParameter(_cameraId, SensorParameter.SensorType, out currentSensor);
            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.Name + " " + _Config.ForeConfig.CameraId + " 获取传感器类型 出错:" + status.ToString());
            }
            if (currentSensor > 1600)
            {
                SetParam( SensorParameter.Gain, Value.Gain);
            }

            int outValue = 0;
            if (Value.AutoHeightZeroPosition)
            {
                status = _fsCore.AdjustRoiAndFps(_cameraId, 0, Value.Freq, out outValue);//0表示有效景深在CMOS上居中
            }
            else
            {
                status = _fsCore.AdjustRoiAndFps(_cameraId, 6, Value.Freq, out outValue);//6表示有效景深关于Z=0对称分布
            }

            SetParam(SensorParameter.PulseWidth, Value.LedPulseWidth);
            SetParam(SensorParameter.PeakThreshold, Value.PeakThreshold);
            SetParam(SensorParameter.PeakXFilter, Value.PeakXFilter);
            SetParam(SensorParameter.FillGapXmax, Value.FillGapXmax);
            SetParam(SensorParameter.MedianZFilterSize, Value.MedianZFilterSize);

            status = _fsCore.StartGrabbing(_Config.ForeConfig.CameraId);

            if (status != CameraStatusCode.Ok)
            {
                throw new Exception(_Config.Name + " " + _Config.ForeConfig.CameraId + " StopGrabbing 出错:" + status.ToString());
            }
        }
    }
}