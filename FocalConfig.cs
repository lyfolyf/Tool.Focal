using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Lead.Tool.Focal
{
    public class Config
    {
        public string Name { get; set; }

        public FocalConfig ForeConfig { get; set; } = new FocalConfig();
    }

    public class FocalConfig
    {
        // 基础参数
        public int SensorType { get; set; } = 1620;
        public double Gain { get; set; }
        public string CameraId { get; set; }
        public int LedPulseWidth { get; set; }
        public int Freq { get; set; }
        public ExportLayer Mode { get; set; }
        // 图像质量参数
        public int PeakThreshold { get; set; }
        public int FirLength { get; set; }
        public int SignalDetectionFilterLength { get; set; }
        public double AverageIntensityFilterSize { get; set; }
        // 滤波参数
        public int NoiseRemoval { get; set; }
        public int PeakXFilter { get; set; }
        // 触发参数
        public bool IsExternalPulsingEnabled { get; set; }         //是否启用外部触发
        public float TrigInterval { get; set; }

        //public int Reordering { get; set; }
        public int RegPulseDivider { get; set; }
        // 毫米与微米转换比例
        public double ProfileScale { get; set; } = 1.0 / 1000.0;
        // focal标定文件，如果这两个参数设置为空，则focal会自动读取机器中已写好的标定文件
        public string ZCalibrationFile { get; set; } = "sensor_calibration_file";
        public string XCalibrationFile { get; set; } = "sensor_x_calibration_file";
        public bool AutoHeightZeroPosition { set; get; }
        //public int PeakAverageIntensityFilterLength { get; set; }
        //public int PeakFirLength { get; set; }
        //public int AverFirLength { get; set; }
        public bool IsXAxisTrige { get; set; }
        //public int ImageHeightZeroPosition { set; get; }
        //public float FlipXX0 { get; set; }
        //public int FlipXEnabled { get; set; }
        //public float DetectMissingFirstLayer { get; set; }
        //public float FillGapXMax { get; set; }
        //public double AverageZFilterSize { get; set; }
        //public Int32 MedianZFilterSize { get; set; }
        //public Int32 MedianIntensityFilterSize { get; set; }
        //public float ResampleLineXResolution { get; set; } //No Param

        public float FillGapXmax { get; set; }
        public int MedianZFilterSize { get; set; }
        public float ClusterMinimumLength { get; set; }
    }

    public class FocalConfigDiffProdution
    {
        [Category("Focal参数"), DisplayName("增益"), Description("不同产品时切换")]
        public double Gain { get; set; } = 2.2;
        [Category("Focal参数"), DisplayName("频率"), Description("不同产品时切换")]
        public int Freq { get; set; } = 3000;
        [Category("Focal参数"), DisplayName("光强"), Description("不同产品时切换")]
        public int LedPulseWidth { get; set; } = 300;
        [Category("Focal参数"), DisplayName("阈值"), Description("不同产品时切换")]
        public int PeakThreshold { get; set; } = 300;
        [Category("Focal参数"), DisplayName("滤波"), Description("不同产品时切换")]
        public int PeakXFilter { get; set; }
        [Category("Focal参数"), DisplayName("孔洞"), Description("不同产品时切换")]
        public float FillGapXmax { get; set; }
        [Category("Focal参数"), DisplayName("中值滤波"), Description("不同产品时切换")]
        public int MedianZFilterSize { get; set; }
        [Category("Focal参数"), DisplayName("true表示有效景深在CMOS上居中,false表示有效景深关于Z=0对称分布"), Description("不同产品时切换")]
        public bool AutoHeightZeroPosition { set; get; }
    }

    //传感器参数名称
    public class SensorParameter
    {
        public const string LedDuration = "led_duration";
        public const string ReorderingDeviation = "reordering_deviation";
        public const string ReorderingSpan = "reordering_span";
        public const string Reordering = "reordering";
        public const string SurfaceMode = "surface_mode";
        public const string Ifg = "ifg";
        public const string Mtu = "mtu";
        public const string SensorTemperature = "sensor_temperature";
        public const string SensorBoardTemperature = "sensor_board_temperature";
        public const string TriggerMaskEnd = "trigger_mask_end";
        public const string TriggerMaskStart = "trigger_mask_start";
        public const string MotorEnableRightEndstop = "motor_enable_right_endstop";
        public const string MotorEnableLeftEndstop = "motor_enable_left_endstop";
        public const string MotorAxisParameterAddr = "motor_parameter_address";
        public const string MotorAxisParameterValueRead = "motor_parameter_value_read";
        public const string MotorAxisParameterValueWrite = "motor_parameter_value_write";
        public const string MotorRotateSpeed = "motor_rotate_speed";
        public const string MotorRotate = "motor_rotate";
        public const string Temperature = "temperature";
        public const string ParamMotorLastAccomplishedCommand = "motor_last_accomplished_command";
        public const string MotorTargetPositionReached = "motor_target_position_reached";
        public const string MotorPosition = "motor_position";
        public const string MotorReadPosition = "motor_read_position";
        public const string MotorReadLeftEndStop = "motor_left_endstop";
        public const string FrequencyCalculation = "frequency_calculation";
        public const string MotorReadRightEndStop = "motor_right_endstop";
        public const string MotorMaxVelocity = "motor_max_velocity";
        public const string MotorMaxAcceleration = "motor_max_acceleration";
        public const string MotorMaxCurrent = "motor_max_current";
        public const string MotorReadAcceleration = "motor_current_acceleration";
        public const string MotorReadEncoderPosition = "motor_encoder_position";
        public const string MotorStop = "motor_stop";
        public const string MotorReadHomeStatus = "motor_home_status";
        public const string MonitorPeriod = "monitor_period";
        public const string DetectMissingFirstLayer = "detect_missing_first_layer";
        public const string DetectMissingFirstLayerLength = "detect_missing_first_layer_min_length";
        public const string ThicknessMode = "thickness_mode";
        public const string LayerIntensityType = "layer_intensity_type";
        public const string LayerMaxThickness = "layer_max_thickness";
        public const string LayerMinThickness = "layer_min_thickness";
        public const string PeakAverageIntensityFilterLength = "peak_average_intensity_filter_length";
        public const string SignalDetectionFilterLength = "signal_detection_filter_length";
        public const string FlushQueue = "flush_queue";
        public const string PeakXFilter = "peak_x_filter";
        public const string NoiseRemoval = "noise_removal";
        public const string Grabbing = "grabbing";
        public const string FrontPanelTemperature = "front_panel_temperature";
        public const string IlluminatorTemperature = "illuminator_temperature";
        public const string ChipTemperature = "chip_temperature";
        public const string SensorBoardTemperatureFloat = "sensor_board_temperature_float";
        public const string BinningFramerate = "reg_binning_framerate";
        public const string LineCallbackMaxLength = "line_callback_max_length";
        public const string LineCallbackXRes = "line_callback_xres";
        public const string FillGapXmax = "fill_gap_x_max";
        public const string ImageHeightZeroPosition = "image_height_zero_position";
        public const string ResampleLineXResolution = "resample_line_x_resolution";
        public const string AverageZFilterSize = "average_z_filter_size";
        public const string AverageIntensityFilterSize = "average_intensity_filter_size";
        public const string MedianZFilterSize = "median_z_filter_size";
        public const string ClusterMinimumLength = "cluster_minimum_length";
        public const string DetectMissingFirstLayerX = "detect_missing_first_layer_x";
        public const string MedianIntensityFilterSize = "median_intensity_filter_size";
        public const string BatchLength = "batch_length";
        public const string BatchTimeout = "batch_timeout";
        public const string DynSensorCtrlEnable = "dyn_sensor_control_enable";
        public const string SensorDataInFlash = "sensor_data_in_flash";
        public const string DeviceSerialNumber = "device_serial_number";
        public const string UserFlashWriteTime = "user_flash_write_time";
        public const string AgcMinPulseWidthLimit = "agc_minimum_pulse_width_limit";
        public const string MotorActive = "motor_active";
        public const string Location = "location";
        public const string FirCoeff = "peak_filter_coeff";
        public const string InterleaveFactor = "interleavefactor";
        public const string Heartbeat = "heartbeat";
        public const string VLow3Float = "hdr_vlow3_float";
        public const string VLow2Float = "hdr_vlow2_float";
        public const string Kp2PosFloat = "hdr_kp2_pos_float";
        public const string Kp1PosFloat = "hdr_kp1_pos_float";
        public const string VLow3 = "hdr_vlow3";
        public const string VLow2 = "hdr_vlow2";
        public const string Vlow2 = "hdr_vlow2";
        public const string Kp2Pos = "hdr_kp2_pos";
        public const string Kp1Pos = "hdr_kp1_pos";
        public const string HdrEnabled = "hdr_enable";
        public const string OffsetY = "image_offsety";
        public const string Height = "image_height";
        public const string Gain = "gain";
        public const string PulseFrequency = "reg_pulse_freq";
        [Obsolete("Recommended variable is SensorParameter.PulseFrequency")]
        public const string Ip = "reg_pulse_freq";
        [Obsolete("Recommended variable is SensorParameter.PulseWidthFloat")]
        public const string Wif = "reg_pulse_width_float";
        public const string PulseWidthFloat = "reg_pulse_width_float";
        public const string PeakEnabled = "peak_enable";
        public const string PeakThreshold = "peak_treshold";
        public const string FirLength = "peak_fir_length";
        public const string FirAverLength = "peak_aver_fir_length";
        public const string InterleaveFilterMode = "interleavefilter";
        public const string PeakCountLimit = "peak_count_limit";
        public const string PeakXUnit = "peak_x_unit";
        public const string ZCalibrationFile = "sensor_calibration_file";
        public const string XCalibrationFile = "sensor_x_calibration_file";
        public const string SensorType = "sensor_type";
        [Obsolete("Recommended variable is SensorParameter.PulseDivider")]
        public const string Di = "reg_pulse_divider";
        public const string PulseDivider = "reg_pulse_divider";
        public const string PeakYUnit = "peak_y_unit";
        public const string InterleaveFilterTargetIntensity = "interleavetargetintensity";
        public const string InterleaveFilterYThreshold = "interleaveythreshold";
        public const string OvercurrentDrive = "overcurrentdrive";
        public const string AvgFrameIntensity = "average_frame_intensity";
        public const string TriggerDebounce = "trigger_debounce";
        public const string FlipX0 = "flip_x_x0";
        public const string FlipXEnabled = "flip_x_enabled";
        public const string FlipZ0 = "flip_z_z0";
        public const string FlipZEnabled = "flip_z_enabled";
        public const string SaturationCount = "saturation_count";
        public const string MaxPointCount = "max_point_count";
        public const string Delay = "reg_pulse_delay";
        [Obsolete("Recommended variable is SensorParameter.PulseWidth")]
        public const string Wi = "reg_pulse_width";
        public const string PulseWidth = "reg_pulse_width";
        public const string Width = "image_width";
        public const string OffsetX = "image_offsetx";
        public const string Vramp = "sensor_vramp";
        public const string Exposure = "exposure";
        public const string LayerEffectiveRefractiveIndex = "layer_refractive_index";
        public const string XLimitMax = "x_limit_max";
        public const string YToleranceMax = "y_tolerance_max";
        public const string PulseCurrentMiddle = "pulse_current_middle";
        public const string PulseCurrentEdges = "pulse_current_edges";
        public const string TriggerSource = "trigger_source";
        public const string TriggerDisableSource = "trigger_disable_source";
        public const string TriggerZeroSource = "trigger_zero_source";
        public const string TriggerMinusSource = "trigger_minus_source";
        public const string XLimitMin = "x_limit_min";
        public const string AgcEnabled = "agc_enabled";
        public const string AgcTarget = "agc_target";
        public const string AgcWiLimit = "agc_pulse_width_limit";
        public const string OutputPinState = "output_pin_state";
        public const string InputPinState = "input_pin_state";
        public const string DutyCycleMonitorPeriod = "duty_cycle_monitor_period";
        public const string YToleranceMin = "y_tolerance_min";
        public const string AgcGain = "agc_gain";

    }
}
