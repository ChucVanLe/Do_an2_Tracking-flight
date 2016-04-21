// Copyright (c) LeVanChuc. All rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls.Maps;
//_____________________________________
using System.Windows.Input;
using System.Windows;

//__________________________________

using System.ComponentModel;
using System.Data;
using Windows.UI.Xaml.Media.Imaging;

using System.Text;
using Windows.UI.Xaml.Shapes;
//khong mo duoc thu vien .NET
//_______________________________
//using Esri.ArcGISRuntime.Layers;
//khong co chuc nang graphic
//khong co add truc tiep string vao map
//khong ve duong tron tu do vao map

using Windows.Services.Maps;
//khong the add system.drawing
//using System.Drawing;
//add win2d.uwp
//khong the add duong thang trong win2d vao map


using Windows.ApplicationModel.Activation;
using Windows.Storage.Pickers;
using System.Numerics;
//Khong sai duoc open cv trong universal app

using System.Runtime.InteropServices;
using System.Threading.Tasks;
//using OpenCvSharp;
//khong the sai opencv Sharp trong project nay


using Windows.Storage;
using Windows.Graphics.Imaging;
using System.Collections;
using System.Diagnostics;


//UART

using System.Collections.ObjectModel;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

using System.Threading;

//**********************************************************************
//**********************************************************************


using System.Globalization;
using WinRTXamlToolkit.Controls;
//using Bing.Maps;

//Class chứa chương trình
using DoAn;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;

namespace SerialSample
{

    //Biến các giá trị từ sensor
    public class DataFromSensor
    {
        public string Acc { get; set; }
        public string Time { get; set; }
        public string Latitude { get; set; }
        public string Longtitude { get; set; }
        public string Speed { get; set; }
        public string Altitude { get; set; }
        public string Angle { get; set; }
        public string Temp { get; set; }
        public string DataAcc { get; set; }
        public string Roll { get; set; }
        public string Pitch { get; set; }
        public string Yaw { get; set; }
    }
    //Biến các giá trị từ sensor
    public class DataFromSensor_Fomat_double
    {
        public double Acc { get; set; }
        public double Time { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; }
        public double Altitude { get; set; }
        public double Angle { get; set; }
        public double Temp { get; set; }
        public double[] DataAcc { get; set; } = new double[30];
    }



    public sealed partial class MainPage : Page
    {

        //global variable
        double dDistanToTaget;  //Save distan from flight to dentination
        double dLatGol, dLonGol;      //2 biến này là biến toàn cục của Lat and Lontitude
        //Geopoint for Seattle San Bay Tan Son Nhat: 10.818442, 106.658824
        public double dLatDentination = 10.818442, dLonDentination = 106.658824;
        Int16 i16EditPosition = -60;//để canh chỉnh màn hình các sensor chỉ hiện 1/3 màn hình
        //1366 x 768 --> 1280 x 800
        //double dConvertToTabletX = 1366 - 1280, dConvertToTabletY = 768 - 800;
        double dConvertToTabletX = 0, dConvertToTabletY = 0;
        bool bDevTablet = false;
        Windows.Storage.StorageFolder storageFolder;
        Windows.Storage.StorageFile sampleFile;
        //string bufferSavedata;//biến này để lưu tạm dữ liệu, bộ đệm cho lưu dữ liệu
        //end of global variable
        public MainPage()
        {
            this.InitializeComponent();

            //*************************************************************************
            //Ngày 29/02/2016 Software Artchitecture
            Dis_Setup();
            try
            {
                //ReadFile();
            }
            catch (Exception ex)
            {

            }

            //test size screen
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);
            var height = size;
        }

        //*****************************************************************
        /*
        Class Set up
        */

        /// <summary>
        /// Set up Map, Timer, Screen
        /// </summary>
        public void Dis_Setup()
        {
            Dis_Setup_MapOffline();
            //C: \Users\VANCHUC - PC\AppData\Local\Packages\54fa2b45 - b04f - 4b40 - 809b - 7556c7ed473f_pq4mhrhe9d4xp\LocalState
            Save_Setup();
            Dis_Setup_UART();
            Dis_Setup_Timer();
            //Set up Display data of sensor
            //Set up position of all component
            DisplaySensor_Setup();

            bSetup = true;

            //chỉ đường offline
            //SetRouteBetween2Point();
            //test
            //slider.ValueChanged += Slider_ValueChanged;


        }

        /// <summary>
        /// Set up MapOffline
        /// </summary>
        public void Dis_Setup_MapOffline()
        {
            myMap.Loaded += MyMap_Loaded;
            //Hien toa do luc nhan chuot trai
            myMap.MapTapped += MyMap_MapTapped;
            //change heading
            myMap.HeadingChanged += MyMap_HeadingChanged;

        }

        /// <summary>
        /// Set up UART
        /// </summary>
        public void Dis_Setup_UART()
        {
            Init_UART();
        }

        /// <summary>
        /// Set up timer read data and show data
        /// </summary>
        public void Dis_Setup_Timer()
        {
            //Set up timer Read Data, period = 1ms
            //InitTimerReadData(2);
            //Set up timer Show Data, period = 500ms

            InitTimerShowData(1000);//Timer này để hiển thị data lên Compass, Speed, Altitude, Roll And Pitch Angle

            //Dữ liệu cập nhật liên tục nhưng chỉ hiện thị sau mỗi 0,5s
            //Vì gia tốc thay đổi nhanh nên ta cần hiển thị sau mỗi 0.1s
            //InitTimerShowAcc(50);

            //ListPortInput(3000);

        }

        /// <summary> check 02/03/16: OK
        /// Khoi tao bien cho ham save and write header
        /// </summary>
        public async void Save_Setup()
        {
            storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
            sampleFile =
                await storageFolder.CreateFileAsync("dataReceive.txt",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
            //write header
            SaveTotxt("Data from sensor Ublox GPS + compass, baud rate: 115200" + '\n');
            SaveTotxt("Test data: 2/3/2016, Location: ... " + '\n');
        }
        //***************End of class set up********************************************
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        //*************Class inside class set up****************************************
        /// <summary>
        /// Load map offline
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMap_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            myMap.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle San Bay Tan Son Nhat:   dLatDentination, dLonDentination

                   Latitude = dLatDentination,
                   Longitude = dLonDentination
               });
            myMap.ZoomLevel = 12;
            myMap.Style = MapStyle.Road;


        }
        /// <summary>
        /// Hiện tọa độ tại điểm ta nhấn chuột
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MyMap_MapTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapInputEventArgs args)
        {
            var tappedGeoPosition = args.Location.Position;
            //string status = "MapTapped at \nLatitude:" + tappedGeoPosition.Latitude + "\nLongitude: " + tappedGeoPosition.Longitude;

            //Show  MapTap to textox
            tb_Lat_Search.Text = Math.Round(tappedGeoPosition.Latitude, 8).ToString();//Lấy 8 chữ số thập phân
            tb_Lon_Search.Text = Math.Round(tappedGeoPosition.Longitude, 8).ToString();//Lấy 8 chữ số thập phân
            //NotifyUser(status, NotifyType.StatusMessage);
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        private void MyMap_HeadingChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            //var tappedGeoPosition = args.Location.Position;
            //string status = "MapTapped at \nLatitude:" + tappedGeoPosition.Latitude + "\nLongitude: " + tappedGeoPosition.Longitude;

            //Show  MapTap to textox
            //tb_Lat_Search.Text = Math.Round(tappedGeoPosition.Latitude, 8).ToString();//Lấy 8 chữ số thập phân
            //tb_Lon_Search.Text = Math.Round(tappedGeoPosition.Longitude, 8).ToString();//Lấy 8 chữ số thập phân
            //NotifyUser(status, NotifyType.StatusMessage);
            //ComPass_Setup_Rotate_Out(myMap.Heading, 350 + i16EditPosition * 11 / 6, 500, 120);//quay phần phía ngoài ok
            ///////////////////////////////////////////////
            Rotate_Needle(myMap.Heading);
        }
        //Ngày 03/12/2015 22h27 đã hoàn thành đọc UART từ serial port
        //Tách dữ liệu và lấy các thông số liên quan
        //chỉ lấy 1 dòng gia tốc trong 10 dòng gia tốc trong 100ms
        //Cứ 100ms có 1 mẫu dữ liệu gồm gia tốc, thơi gian, lat, long, alt, angle
        //***********************************************************************
        //*********************************************************************
        //Ngay 02/12/2015****************************************************
        //*******************Read And Display Data***************************
        //https://ms-iot.github.io/content/en-US/win10/samples/SerialSample.htm

        //https://msdn.microsoft.com/en-us/library/windows.devices.serialcommunication.serialdevice.aspx

        /// <summary>
        /// Private variables
        /// </summary>
        ///
        //Cac bien toan cuc dung cho chuong trinh UART 
        //Var in UART
        string strDataFromSerialPort = "";
        bool bConnectOk = false;
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;

        private ObservableCollection<DeviceInformation> listOfDevices;
        private CancellationTokenSource ReadCancellationTokenSource;
        public void Init_UART()
        {
            comPortInput.IsEnabled = false;
            //sendTextButton.IsEnabled = false;
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
        }

        /// <summary>
        /// Timer để đọc data from Sensor
        /// dPeriod là chu kỳ timer đơn vị ms
        /// </summary>
        private void InitTimerReadData(double dPeriod)
        {


            // Start the polling timer.
            TimerReadData = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(dPeriod) };
            TimerReadData.Tick += TimerReadData_Tick;
            TimerReadData.Start();

        }

        /// <summary>
        /// Timer để Show data to Compass, Alt, Speed, Roll, Pitch,.. 
        /// </summary>
        private void InitTimerShowData(double dPeriod)
        {


            // Start the polling timer.
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(dPeriod) };
            timer.Tick += TimerShowData;
            timer.Start();

        }
        //*******************************************************************

        /// <summary>
        /// Vi gia toc thay doi nhanh nen ta se update data nhanh hon
        /// nen ta dung 2 timer, update acc sau mỗi 100ms, ghi luôn thời gian nhận được, xem có trễ k
        /// </summary>
        /// <param name="dPeriod"></param>
        private void InitTimerShowAcc(double dPeriod)
        {


            // Start the polling timer.
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(dPeriod) };
            timer.Tick += Timer_ShowAcc_Tick;
            timer.Start();

        }

        /// <summary>
        /// Liệt kê các port đang connect với máy tính
        /// </summary>
        /// <param name="dPeriod"></param>
        private void ListPortInput(double dPeriod)
        {


            // Start the polling timer.
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(dPeriod) };
            timer.Tick += listPort;
            timer.Start();

        }

        public void listPort(object sender, object e)
        {
            ListAvailablePorts();
        }

        //*****************************************************************
        //Hàm set up tòan bộ cảm biến
        /// <summary>
        /// Set up vị trí hiển thị của cảm biến
        /// </summary>
        void DisplaySensor_Setup()
        {
            
            Background_Sensor(480, -80);//da can chinh 1/3 full screen
            //Ve them speed
            //Speed_Image_Setup(0, 150, 0);

            //Speed_Setup(00, 150, 100);
            //Thay đổi speed bằng cách xoay ảnh
            //AirSpeed_Image_Setup(0, 150 - 32, 100 + 125);//đã canh đúng trung tâm 12/3/2016
            //Image full
            //Da can chinh 1/3
            AirSpeed_Image_full_Setup(100.1, 150 - 32 + i16EditPosition, 80 + 125);//ok
            Draw_Airspeed_full_optimize(100.012, 150 - 32 + i16EditPosition, 205);//ok500, 120
                                                                                  //Speed_Image_Setup(100, 150, 100);
                                                                                  //Da can chinh 1/3
            PitchAndRoll_Setup(0, 0, 350 + i16EditPosition * 11 / 6, 200, 140, 50);//ok
            PitchAndRoll_Draw(45, 0, 350 + i16EditPosition * 11 / 6, 200, 140, 50);//ok

            //Altitude_Setup(00, 550, 100);
            //Vẽ hình Altitude
            //Altitude_Image_Setup(00, 550, 100); //đã vẽ xong lúc 1h25 13/3/2016
            //Da can chinh 1/3
            Alttitude_Image_full_Setup(100.5, 550 + 88 / 2 + i16EditPosition * 17 / 6, 80);//ok
            Draw_Alttitude_full_optimize(0, 550 + 88 / 2 + i16EditPosition * 17 / 6, 80);//ok
            //Da can chinh 1/3
            ComPass_Setup_Rotate_Out(0, 350 + i16EditPosition * 11 / 6, 500, 120);//quay phần phía ngoài ok
            //Da can chinh 1/3
            VerticalSpeed_Setup(0, 550 + i16EditPosition * 17 / 6, 420);

            //set up quy dao
            Draw_Trajectory_And_Flight(dLatGol, dLonGol,
                        Convert.ToDouble(Data.Altitude), Convert.ToDouble("0"));//ok

            //Set up Show distan
            ShowDistance(0, 0, dDistanToTaget.ToString() + " Meter", 30 * myMap.ZoomLevel / 22, dLatGol, dLonGol, 1);//Purple

            ///////////////////////////////////////////////////////////////////
            //Add needle
            AddNeedle(1270 - dConvertToTabletX, 660 - dConvertToTabletY);
        }
        //*************End Of Class inside class set up****************************************

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        //*************Class inside inside class set up****************************************

        //UART*********************************************************************************
        /// <summary>
        /// ListAvailablePorts
        /// - Use SerialDevice.GetDeviceSelector to enumerate all serial devices
        /// - Attaches the DeviceInformation to the ListBox source so that DeviceIds are displayed
        /// </summary>
        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                //status.Text = "Select a device and connect";
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Remove(dis[i]);
                }
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                DeviceListSource.Source = listOfDevices;
                comPortInput.IsEnabled = true;
                ConnectDevices.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
            }
        }

        /// <summary>
        /// chỉnh tốc độ baud
        /// comPortInput_Click: Action to take when 'Connect' button is clicked
        /// - Get the selected device index and use Id to create the SerialDevice object
        /// - Configure default settings for the serial port
        /// - Create the ReadCancellationTokenSource token
        /// - Start listening on the serial port input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void comPortInput_Click(object sender, RoutedEventArgs e)
        {
            var selection = ConnectDevices.SelectedItems;

            if (selection.Count <= 0)
            {
                //status.Text = "Select a device and connect";
                return;
            }

            DeviceInformation entry = (DeviceInformation)selection[0];

            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);

                // Disable the 'Connect' button 
                comPortInput.IsEnabled = false;

                // Configure serial settings
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1);
                serialPort.BaudRate = 115200;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                //// Display configured settings
                //status.Text = "Serial port configured successfully: ";
                //status.Text += serialPort.BaudRate + "-";
                //status.Text += serialPort.DataBits + "-";
                //status.Text += serialPort.Parity.ToString() + "-";
                //status.Text += serialPort.StopBits;

                //// Set the RcvdText field to invoke the TextChanged callback
                //// The callback launches an async Read task to wait for data
                //rcvdText.Text = "Waiting for data...";
                //Connect is successfull
                bConnectOk = true;
                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();

                // Enable 'WRITE' button to allow sending data
                //sendTextButton.IsEnabled = true;


                Listen();
            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
                comPortInput.IsEnabled = true;
                //sendTextButton.IsEnabled = false;
            }
        }
        //**************************************************************************
        /// <summary>
        /// sendTextButton_Click: Action to take when 'WRITE' button is clicked
        /// - Create a DataWriter object with the OutputStream of the SerialDevice
        /// - Create an async task that performs the write operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void sendTextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serialPort != null)
                {
                    // Create the DataWriter object and attach to OutputStream
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    //Launch the WriteAsync task to perform the write
                    await WriteAsync();
                }
                else
                {
                    //status.Text = "Select a device and connect";
                }
            }
            catch (Exception ex)
            {
                //status.Text = "sendTextButton_Click: " + ex.Message;
            }
            finally
            {
                // Cleanup once complete
                if (dataWriteObject != null)
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }
        //**************************************************************************
        /// <summary>
        /// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        /// </summary>
        /// <returns></returns>
        private async Task WriteAsync()
        {
            Task<UInt32> storeAsyncTask;

            //if (sendText.Text.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                //dataWriteObject.WriteString(sendText.Text);

                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                    //status.Text = sendText.Text + ", ";
                    //status.Text += "bytes written successfully!";
                }
                //sendText.Text = "";
            }
            //else
            {
                //status.Text = "Enter the text you want to write and then click on 'WRITE'";
            }
        }
        //**************************************************************************
        //dem so frame loi
        int errorFrame = 0;
        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    //status.Text = "Reading task was cancelled, closing device and cleaning up";
                    CloseDevice();
                }
                else
                {
                    //status.Text = ex.Message;
                    //loi frame
                }




            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }


        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;//savedatatoTxtFile để lưu data --> .txt
            //nếu bộ đệm lớn sẽ có thời gian trễ lớn, nhưng dữ liệu không mất
            uint ReadBufferLength = 1;//2000 char

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            //save --> .txt
            //savedatatoTxtFile = dataReaderObject.LoadAsync(2000).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            //save
            //UInt32 bytesSave = await savedatatoTxtFile;

            //if (bytesSave > 0)
            //{
            //    rcvdText.Text = dataReaderObject.ReadString(bytesRead);
            //    strDataFromSerialPort += rcvdText.Text;
            //    //dataReaderObject.r
            //    //status.Text = "bytes read successfully!";

            //    //Save data to .txt file
            //    SaveTotxt(rcvdText.Text);

            //}
            //Process and Save
            string sTemp;//dung để process and save
            if (bytesRead > 0)
            {
                //rcvdText.Text = dataReaderObject.ReadString(bytesRead);
                sTemp = dataReaderObject.ReadString(bytesRead);
                //dataReaderObject.r
                //status.Text = "bytes read successfully!";
                ////Save data to .txt file
                //bufferSavedata += rcvdText.Text;

                strDataFromSerialPort += sTemp;
                //Process Data
                try
                {
                    //if ((strDataFromSerialPort != " ") && (strDataFromSerialPort != ""))
                        ProcessData();
                }
                catch(Exception ex)
                {

                    errorFrame += 1;
                    tb_ShowTime.Text = "frame error: " + strDataFromSerialPort + "Error: " + ex.Message + "No Error: " + errorFrame.ToString();

                }
                //
                //SaveTotxt(sTemp);
                //show now time
                tb_ShowTime.Text = "Now time: " + Data.Time;

            }
        }
        //****************************************************************************
        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;

            comPortInput.IsEnabled = true;
            //sendTextButton.IsEnabled = false;
            //rcvdText.Text = "";
            listOfDevices.Clear();
        }
        //***************************************************************************
        /// <summary>
        /// closeDevice_Click: Action to take when 'Disconnect and Refresh List' is clicked on
        /// - Cancel all read operations
        /// - Close and dispose the SerialDevice object
        /// - Enumerate connected devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeDevice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //status.Text = "";
                CancelReadTask();
                CloseDevice();
                ListAvailablePorts();
            }
            catch (Exception ex)
            {
                //status.Text = ex.Message;
            }
        }
        //***************************************************************************
        //Receice Data
        private async void ReceiveData_Click(object sender, RoutedEventArgs e)
        {
            // read the data

            DataReader dreader = new DataReader(serialPort.InputStream);
            uint sizeFieldCount = await dreader.LoadAsync(sizeof(uint));
            if (sizeFieldCount != sizeof(uint))
            {
                return;
            }

            uint stringLength;
            uint actualStringLength;

            try
            {
                stringLength = dreader.ReadUInt32();
                actualStringLength = await dreader.LoadAsync(stringLength);

                if (stringLength != actualStringLength)
                {
                    return;
                }
                string text = dreader.ReadString(actualStringLength);

                //message.Text = text;

            }
            catch
            {
                //errorStatus.Visibility = Visibility.Visible;
                //errorStatus.Text = "Reading data from Bluetooth encountered error!" + ex.Message;
            }


        }
        //End of all function of UART**************************************************************



        //add icon to map
        void Add_Icon_MyHome()
        {
            MapIcon icon = new MapIcon();
            Geopoint pointCenter = new Geopoint(new BasicGeoposition()
            {
                //Tọa độ nhà Lê Văn Chức
                Latitude = 15.235057,
                Longitude = 108.742786,
                // Altitude = 200.0
            });
            //icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/MyHome.png"));
            icon.Location = pointCenter;

            icon.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;

            icon.Title = "My Home";

            myMap.MapElements.Add(icon);


        }
        //**************************************************************************
        /// <summary>
        /// Load Background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMap_Loaded_Background(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MapBackground.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle San Bay Tan Son Nhat:dLatDentination, dLonDentination
                   Latitude = dLatDentination,
                   Longitude = dLonDentination
               });
            MapBackground.ZoomLevel = 17;
            MapBackground.Style = MapStyle.Road;
            //MapBackground.IsEnabled = false;

            //MapBackground.i
            //MapBackground.ha
            //MapBackground.bl
            //Canh chỉnh 2 map
            //myMap.Margin = new Windows.UI.Xaml.Thickness(700, 0, 00, 00);
            //MapBackground.Width = 700;

        }
        //add Polygon to map

        private void mapPolygonAddButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //center of map
            double centerLatitude = myMap.Center.Position.Latitude;
            double centerLongitude = myMap.Center.Position.Longitude;

            //__________Test Function draw circle
            //lay vi do
            double TestCenterLat = myMap.Center.Position.Latitude + 0.0005;
            //Lay kinh do
            double TestCenterLog = myMap.Center.Position.Longitude + 0.001;
            //Ban kinh
            //double Radius = 0.01;
            //Goc angpha
            //double angle;
            //ve duong tron moi
            Windows.UI.Xaml.Controls.Maps.MapPolygon DrawRectangle = new Windows.UI.Xaml.Controls.Maps.MapPolygon();
            DrawRectangle.ZIndex = 1;
            //mau nen
            DrawRectangle.FillColor = Windows.UI.Color.FromArgb(100, 255, 0, 0);
            DrawRectangle.StrokeColor = Colors.Green;
            DrawRectangle.StrokeThickness = 3;
            DrawRectangle.StrokeDashed = false;

            DrawRectangle.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude=centerLatitude+0.0005, Longitude=centerLongitude-0.001, Altitude = 00 },
                //new BasicGeoposition() {Latitude=centerLatitude-0.0005, Longitude=centerLongitude-0.001, Altitude = 00 },
                new BasicGeoposition() {Latitude=centerLatitude-0.0005, Longitude=centerLongitude+0.001, Altitude = 100 },
                new BasicGeoposition() {Latitude=centerLatitude+0.0005, Longitude=centerLongitude+0.001, Altitude = 30 },
                });
            //clear map
            myMap.MapElements.Clear();
            myMap.Children.Clear();

            //******************************************************************
            //****************************************************************
            //____________Test Function_________________
            //add hinh chu nhat
            //myMap.MapElements.Add(DrawRectangle);
            // Constants and helper functions:
            //DrawCircle(new BasicGeoposition() { Latitude = centerLatitude, Longitude = centerLongitude }, 0.01);
            //DrawTextNoDepenOnZoom(200.0, 300.0, 420, 800, "CHÚC THẦY 20 - 11 VUI VẺ");
            //DrawTextDepenOnZoom();
            //Map_DrawLine(1, 1, 1, 1, 1, 1);
            //DrawRoute3DInMap();
            //SetRouteDirectionsBreda();
            //SetRouteBetween2Point();
            //showMap3D();
            //Ngày 28/11/2015 Test Win2D
            // LoadImage();
            //TestWin2D();
            //Image_Loaded();
            //IsolatedStorage();
            //Ngay 01/12/2015
            // Test();
            //Ngày 03/12/2015
            //Image_TestEdit();
            //RotateTransformSample1();
            //Background_Sensor();

        }
        /************************************************/
        // Function to draw circle on map:

        private void DrawCircle(BasicGeoposition CenterPosition, double Radius)
        {
            //tạo màu ở trong đường tròn
            Color FillColor = Colors.Blue;
            //tạo màu cho đường tròn
            Color StrokeColor = Colors.Red;
            //độ đục của màu
            FillColor.A = 80;
            StrokeColor.A = 80;
            Windows.UI.Xaml.Controls.Maps.MapPolygon Circle = new Windows.UI.Xaml.Controls.Maps.MapPolygon
            {
                //độ dày của đường tròn
                StrokeThickness = 2,
                FillColor = FillColor,
                StrokeColor = StrokeColor,
                //những điểm màu đường tròn đó đi qua
                Path = new Geopath(CalculateCircle(CenterPosition, Radius))
            };
            myMap.MapElements.Add(Circle);
            //DynamicLabelingInfo minorCityLabelInfo = new DynamicLabelingInfo();
            //minorCityLabelInfo.LabelExpression = "[areaname]";
            //Graphic g = new Graphic();
            //myMap.MapElements.Add(g);
            //myMap.Children.Add(minorCityLabelInfo);
        }
        // Constants and helper functions:
        public static List<BasicGeoposition> CalculateCircle(BasicGeoposition Position, double Radius)
        {
            //const double earthRadius = 6371000D;
            //const double Circumference = 2D * Math.PI * earthRadius;
            List<BasicGeoposition> GeoPositions = new List<BasicGeoposition>();
            for (int i = 0; i <= 360; i++)
            {
                //chuyển từ độ sang radian
                double Bearing = ToRad(i);
                //tạo 1 điểm thuộc đường tròn
                BasicGeoposition NewBasicPosition = new BasicGeoposition();
                NewBasicPosition.Latitude = Position.Latitude + Radius * Math.Cos(Bearing);
                NewBasicPosition.Longitude = Position.Longitude + Radius * Math.Sin(Bearing);
                //add điểm đó vào list các điểm đường tròn đi qua
                GeoPositions.Add(NewBasicPosition);
            }
            return GeoPositions;
        }

        private static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180D);
        }
        /***********************************************/
        //add String to map
        // Create a label
        // Minor city label info

        //DynamicLabelingInfo minorCityLabelInfo = new DynamicLabelingInfo();
        //Vẽ string trong map
        void DrawTextNoDepenOnZoom(double DistanceFromLeft, double DistanceFromTop, Int32 HeightOfBlock, Int32 WidthOfBlock, string text)
        {
            //create graphic text block design text
            TextBlock TxtDesign = new TextBlock();
            //chiều dài rộng của khung chứa text
            TxtDesign.Height = HeightOfBlock;
            TxtDesign.Width = WidthOfBlock;
            //canh lề, left, right, center
            TxtDesign.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesign.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesign.Margin = 
            //
            //đảo chữ
            TxtDesign.TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            TxtDesign.Text = text;
            TxtDesign.FontSize = 40;
            //color text có độ đục
            TxtDesign.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //position of text left, top, right, bottom
            TxtDesign.Margin = new Windows.UI.Xaml.Thickness(DistanceFromLeft, DistanceFromTop, 0, 0);
            myMap.Children.Add(TxtDesign);

            //MapLayer.SetPosition(tb, location);

            //grid.Children.Add(TxtDesign);
            //myMap.MapElements.Add(TxtDesign);
            //this.GetTemplateChild
            //MapControl.SetLocation()
        }

        void DrawTextDepenOnZoom()
        {
            Grid pin = new Grid()
            {
                Width = 150,
                Height = 150,
                Margin = new Windows.UI.Xaml.Thickness(200, 200, 0, 0)


            };
            pin.Children.Add(new TextBlock()
            {
                Text = "Le Van Chuc",
                FontSize = 20,
                Foreground = new SolidColorBrush(Colors.Red),

                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center,
                SelectionHighlightColor = new SolidColorBrush(Windows.UI.Colors.Green),
                //Foreground = new SolidColorBrush(Windows.UI.Colors.Blue),
                FontFamily = new FontFamily("Arial"),
                FontStyle = Windows.UI.Text.FontStyle.Italic



            });

            //vị trí trung tâm của map không thay đổi nên ta sẽ lấy nó làm hệ quy chiếu
            MapControl.SetLocation(pin, new Geopoint(
                new BasicGeoposition()
                {
                    Latitude = myMap.Center.Position.Latitude + 0.005,
                    Longitude = myMap.Center.Position.Longitude - 0.001
                }));
            myMap.Children.Add(pin);
        }
        /*************************Ngay 21/11/2015*******************************/
        //Calculate distance in map
        /*
         * Calculate distance between two points in latitude and longitude taking
         * into account height difference. If you are not interested in height
         * difference pass 0.0. Uses Haversine method as its base.
         * 
         * lat1, lon1 Start point lat2, lon2 End point el1 Start altitude in meters
         * el2 End altitude in meters
         * @returns Distance in Meters
         */
        public static Int32 distance(double lat1, double lon1, double el1,
        double lat2, double lon2, double el2)
        {

            //Int16 R = 6371; // Radius of the earth unit km
            //Do trai đất elip nên để chính xác lấy
            Int32 R = 6372803;

            double latDistance = ToRad(lat2 - lat1);//convert degree to radian
            double lonDistance = ToRad(lon2 - lon1);

            double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
                    + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                    * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            //double distance = R * c * 1000; // convert to meters
            double distance = R * c;
            double height = el1 - el2;

            distance = Math.Pow(distance, 2) + Math.Pow(height, 2);

            return (Int32)Math.Sqrt(distance);
        }
        //**************************************************************************
        //Show distance
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_ShowDistance = new TextBlock[2];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// Roll góc nghiêng của string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void ShowDistance(int index, double Roll, string drawString, double SizeOfText,
            double lat, double lon, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_ShowDistance[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_ShowDistance[index].Height = HeightOfBlock;
            //Tb_ShowDistance[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            myMap.Children.Remove(Tb_ShowDistance[index]);
            Tb_ShowDistance[index] = new TextBlock();
            Tb_ShowDistance[index].HorizontalAlignment = HorizontalAlignment.Right;
            Tb_ShowDistance[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_ShowDistance[index].Margin = 
            //
            //đảo chữ
            Tb_ShowDistance[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_ShowDistance[index].Text = drawString;
            Tb_ShowDistance[index].FontSize = SizeOfText;
            Tb_ShowDistance[index].FontFamily = new FontFamily("Arial");
            //Tb_ShowDistance[index].FontStyle = "Arial";
            //Tb_ShowDistance[index].FontStretch
            //color text có độ đục
            //Tb_ShowDistance[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //Tb_ShowDistance[index].Foreground = Blush;
            Tb_ShowDistance[index].Foreground = new SolidColorBrush(Colors.Red);
            Tb_ShowDistance[index].Opacity = Opacity;
            //Quay Textblock để quay chữ
            Tb_ShowDistance[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll + 180,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };
            //position of text left, top, right, bottom
            //Tb_ShowDistance[index].Margin = new Windows.UI.Xaml.Thickness(100, 20, 0, 0);
            //BackgroundDisplay.Children.Add(Tb_ShowDistance[index]);

            //Đặt theo tọa độ
            //Tan Son Nhat Airport dLatDentination, dLonDentination
            Geopoint Position = new Geopoint(new BasicGeoposition()
            {
                //dLatDentination, dLonDentination
                //Latitude = dLatDentination,
                //Longitude = dLonDentination,
                //Altitude = 200.0
                //Latitude = dLatDentination + (lat - dLatDentination) * (slider.Value / 1000 + 0.9),
                //Longitude = dLonDentination + (lon - dLonDentination) * (slider.Value / 1000 + 0.9),

                //C2
                //Latitude = dLatDentination + (lat - dLatDentination) * (0.0169 * myMap.ZoomLevel + 0.6597),
                //Longitude = dLonDentination + (lon - dLonDentination) * (0.0169 * myMap.ZoomLevel + 0.6597),

                //C3: Quay chu them 180D
                Latitude = lat,
                Longitude = lon,
            });
            //myMap.Children.Add(bitmapImage);
            //Đặt đúng vị trí
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(Tb_ShowDistance[index], Position);
            myMap.Children.Add(Tb_ShowDistance[index]);

            //Show Zool Level
            tb_ZoomLevel.Text = "Zoom Level: " + Math.Round(myMap.ZoomLevel, 3).ToString();
        }
        //**********************************************************************************************
        public void ShowDistance_optimize(int index, double Roll, string drawString, double SizeOfText,
            double lat, double lon)
        {
            //create graphic text block design text
            //TextBlock Tb_ShowDistance[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_ShowDistance[index].Height = HeightOfBlock;
            //Tb_ShowDistance[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            myMap.Children.Remove(Tb_ShowDistance[index]);
            //Tb_ShowDistance[index] = new TextBlock();
            //Tb_ShowDistance[index].HorizontalAlignment = HorizontalAlignment.Right;
            //Tb_ShowDistance[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_ShowDistance[index].Margin = 
            //
            //đảo chữ
            //Tb_ShowDistance[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_ShowDistance[index].Text = drawString;
            Tb_ShowDistance[index].FontSize = SizeOfText;
            //Tb_ShowDistance[index].FontFamily = new FontFamily("Arial");
            //Tb_ShowDistance[index].FontStyle = "Arial";
            //Tb_ShowDistance[index].FontStretch
            //color text có độ đục
            //Tb_ShowDistance[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //Tb_ShowDistance[index].Foreground = Blush;
            //Tb_ShowDistance[index].Foreground = new SolidColorBrush(Colors.Purple);
            //Tb_ShowDistance[index].Opacity = Opacity;
            //Quay Textblock để quay chữ
            Tb_ShowDistance[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll + 180,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };
            //position of text left, top, right, bottom
            //Tb_ShowDistance[index].Margin = new Windows.UI.Xaml.Thickness(100, 20, 0, 0);
            //BackgroundDisplay.Children.Add(Tb_ShowDistance[index]);

            //Đặt theo tọa độ
            //Tan Son Nhat Airport dLatDentination, dLonDentination
            Geopoint Position = new Geopoint(new BasicGeoposition()
            {
                //dLatDentination, dLonDentination
                //Latitude = dLatDentination,
                //Longitude = dLonDentination,
                //Altitude = 200.0
                //Latitude = dLatDentination + (lat - dLatDentination) * (slider.Value / 1000 + 0.9),
                //Longitude = dLonDentination + (lon - dLonDentination) * (slider.Value / 1000 + 0.9),

                //C2
                //Latitude = dLatDentination + (lat - dLatDentination) * (0.0169 * myMap.ZoomLevel + 0.6597),
                //Longitude = dLonDentination + (lon - dLonDentination) * (0.0169 * myMap.ZoomLevel + 0.6597),

                //C3: Quay chu them 180D
                Latitude = lat,
                Longitude = lon,
            });
            //myMap.Children.Add(bitmapImage);
            //Đặt đúng vị trí
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(Tb_ShowDistance[index], Position);
            myMap.Children.Add(Tb_ShowDistance[index]);

            //Show Zool Level
            tb_ZoomLevel.Text = "Zoom Level: " + Math.Round(myMap.ZoomLevel, 3).ToString();
        }
        //**********************************************************************************************

        //My home 15.235020N, 108.742780E 24.12m
        //wiew map 3D
        private async void AddMap3D(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //myMap.Heading = 10;
            myMap.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle 
                   Latitude = 15.235057,
                   Longitude = 108.742786,
               });
            myMap.ZoomLevel = 10;
            myMap.Style = MapStyle.Road;
            //wiew map 3D 1000, 0, 90
            Geopoint point = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude
            });
            await myMap.TrySetSceneAsync(MapScene.CreateFromLocationAndRadius(point, 1000, 0, 0));//0: phuong cua ban do, 0 là hướng bắc, 45 độ nghiêng
        }
        //******************Map 3D*************************
        private async void display3DLocation(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (myMap.Is3DSupported)
            {
                // Set the aerial 3D view.
                myMap.Style = MapStyle.Aerial3DWithRoads;

                // Specify the location.
                BasicGeoposition hwGeoposition = new BasicGeoposition() { Latitude = 34.134, Longitude = -118.3216 };
                Geopoint hwPoint = new Geopoint(hwGeoposition);

                // Create the map scene.
                MapScene hwScene = MapScene.CreateFromLocationAndRadius(hwPoint,
                                                                                     80, /* show this many meters around */
                                                                                     0, /* looking at it to the North*/
                                                                                     60 /* degrees pitch */);
                // Set the 3D view with animation.
                await myMap.TrySetSceneAsync(hwScene, MapAnimationKind.Bow);
            }
            else
            {
                // If 3D views are not supported, display dialog.
                ContentDialog viewNotSupportedDialog = new ContentDialog()
                {
                    Title = "3D is not supported",
                    Content = "\n3D views are not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                await viewNotSupportedDialog.ShowAsync();
            }
        }

        //draw line in map
        void Map_DrawLine(double lat1, double lat2, double lon1,
        double lon2, double el1, double el2)
        {
            double centerLatitude = myMap.Center.Position.Latitude;
            double centerLongitude = myMap.Center.Position.Longitude;
            Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
            mapPolyline.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude=centerLatitude + 0.00005 + 0.00001 * myMap.ZoomLevel, Longitude=centerLongitude + 0.000001 * (22- myMap.ZoomLevel), Altitude = 200 },
                new BasicGeoposition() {Latitude=centerLatitude + 0.05, Longitude=centerLongitude + 0.05, Altitude = 400 },
            });

            mapPolyline.StrokeColor = Colors.Black;
            mapPolyline.StrokeThickness = 5;
            mapPolyline.StrokeDashed = true;
            myMap.MapElements.Add(mapPolyline);

        }
        //draw line in map 2D
        //đường thẳng đưa vào là biến toàn cục
        Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
        /// <summary>
        /// Ve duong thẳng nối (lat1, lon1) va (lat2, lon2)
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        void Map_DrawLine_2D(double lat1, double lon1,
        double lat2, double lon2)
        {

            myMap.MapElements.Remove(mapPolyline);
            mapPolyline.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude = lat1, Longitude = lon1},
                //San Bay Tan Son Nhat: dLatDentination, dLonDentination
                new BasicGeoposition() {Latitude = lat2, Longitude = lon2},
            });

            //mapPolyline.StrokeColor = Colors.Red;
            //mapPolyline.StrokeThickness = 2;
            //mapPolyline.StrokeDashed = false;//nét liền
            myMap.MapElements.Add(mapPolyline);

        }
        //*****************************************
        public async void DrawRoute3DInMap()
        {
            Geopoint point1 = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude,
                //Altitude = 200.0
            });
            Geopoint point2 = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude + 0.001,
                Longitude = myMap.Center.Position.Longitude,
                //Altitude = 200.0
            });
            BasicGeoposition startLocation = new BasicGeoposition();
            startLocation.Latitude = 40.7517;
            startLocation.Longitude = -073.9766;
            Geopoint startPoint1 = new Geopoint(startLocation);

            // End at Central Park in New York City.
            BasicGeoposition endLocation1 = new BasicGeoposition();
            endLocation1.Latitude = 40.7669;
            endLocation1.Longitude = -073.9790;
            Geopoint endPoint1 = new Geopoint(endLocation1);

            // Get the route between the points.

            MapRouteFinderResult routeResult =
               await MapRouteFinder.GetDrivingRouteAsync(startPoint1, endPoint1,
                MapRouteOptimization.TimeWithTraffic,
                MapRouteRestrictions.None);
            // MapRouteFinderResult router = await MapRouteFinder.GetDrivingRouteAsync(;



            // Fit the MapControl to the route.

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                //MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Red;
                viewOfRoute.OutlineColor = Colors.Blue;

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.
                myMap.Routes.Add(viewOfRoute);
                // Use the route to initialize a MapRouteView.
                viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;
                // Fit the MapControl to the route.
                await myMap.TrySetViewBoundsAsync(
                    routeResult.Route.BoundingBox,
                    null,
                    Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.

            }

            if (routeResult.Status == MapRouteFinderStatus.Success)


            {


                //(routeResult.Route.LengthInMeters / 1000);


            }

        }

        //****************************************************
        public async void SetRouteDirectionsBreda()
        {
            //ok
            string beginLocation = "ho chi minh";
            string endLocation = "Binh Duong";

            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(beginLocation, myMap.Center);
            MapLocation begin = result.Locations.First();

            result = await MapLocationFinder.FindLocationsAsync(endLocation, myMap.Center);
            MapLocation end = result.Locations.First();

            List<Geopoint> waypoints = new List<Geopoint>();
            waypoints.Add(begin.Point);
            // Adding more waypoints later
            waypoints.Add(end.Point);
            //Add point


            MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(begin.Point, end.Point, MapRouteOptimization.Time, MapRouteRestrictions.None);

            //System.Diagnostics.Debug.WriteLine(routeResult.Status); // DEBUG

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Green;
                viewOfRoute.OutlineColor = Colors.Black;

                myMap.Routes.Add(viewOfRoute);

                await myMap.TrySetViewBoundsAsync(routeResult.Route.BoundingBox, null, MapAnimationKind.None);
            }
            else
            {
                // throw new Exception(routeResult.Status.ToString());
            }
        }
        ////****************************Ngay 22/11/2015************************
        //chỉ đường chỉ thực hiện online khi nhap vao dia diem
        //khi nhap toa do thi co the search offline
        //Chi đường chỉ làm được với 1 số địa điểm được đặt tên trên bản đồ như chi đường giữa 2 tỉnh, từ tp hcm
        //đến sân bay nha trang "Sân Bay Nha Trang, Khanh Hoa, Vietnam"
        //san bay tan son nhat "58 Truong Son, Ward 2, Tan Binh District Ho Chi Minh City  Ho Chi Minh City"
        //          endLocation2.Latitude = 10.772099;
        //          endLocation2.Longitude = 106.657693;
        //San bay tan son nhat dLatDentination, dLonDentination
        //san bay da nang 16.044040, 108.199357
        public async void SetRouteBetween2Point()
        {
            //ok
            string beginLocation = "ho chi minh";
            string endLocation = "ha noi";

            // End at Central in Binh Duong
            BasicGeoposition endLocation1 = new BasicGeoposition();
            endLocation1.Latitude = 11.216412;
            endLocation1.Longitude = 106.957936;
            Geopoint endPoint1 = new Geopoint(endLocation1);
            // End at Central in Tan Son Nhat InterNational AirPort: dLatDentination, dLonDentination
            BasicGeoposition endLocation2 = new BasicGeoposition();
            endLocation2.Latitude = 10.759860;
            endLocation2.Longitude = 106.92668;
            endLocation2.Altitude = 100;
            Geopoint endPoint2 = new Geopoint(endLocation2);
            //position: string
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAsync(beginLocation, myMap.Center);
            MapLocation begin = result.Locations.First();

            result = await MapLocationFinder.FindLocationsAsync(endLocation, myMap.Center);
            MapLocation end = result.Locations.First();

            List<Geopoint> waypoints = new List<Geopoint>();
            waypoints.Add(begin.Point);
            // Adding more waypoints later
            waypoints.Add(end.Point);

            MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(begin.Point, end.Point, MapRouteOptimization.Distance, MapRouteRestrictions.Highways);
            //test show point
            tb_ZoomLevel.Text = begin.Point.Position.Latitude.ToString() + "  "
                                + begin.Point.Position.Longitude.ToString();
            //System.Diagnostics.Debug.WriteLine(routeResult.Status); // DEBUG

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Green;
                viewOfRoute.OutlineColor = Colors.Black;

                myMap.Routes.Add(viewOfRoute);

                await myMap.TrySetViewBoundsAsync(routeResult.Route.BoundingBox, null, MapAnimationKind.None);
            }
            else
            {
                // throw new Exception(routeResult.Status.ToString());
            }
        }
        //map 3D
        private async void showMap3D_Aerial3DWithRoads()
        {
            myMap.Style = MapStyle.Aerial3DWithRoads;
            //wiew map 3D 1000, 0, 90
            Geopoint point = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude
            });
            await myMap.TrySetSceneAsync(MapScene.CreateFromLocationAndRadius(point, 1000, 0, 80));//0: phuong cua ban do, 0 là hướng bắc, 45 độ nghiêng
                                                                                                   // DrawRoute3DInMap();
        }
        //***********************************************
        private async void showMap3D_Roads()
        {
            //myMap.Style = MapStyle.Aerial3DWithRoads;
            ////wiew map 3D 1000, 0, 90
            //Geopoint point = new Geopoint(new BasicGeoposition()
            //{
            //    Latitude = myMap.Center.Position.Latitude,
            //    Longitude = myMap.Center.Position.Longitude
            //});
            //await myMap.TrySetSceneAsync(MapScene.CreateFromLocationAndRadius(point, 1000, 0, 50));//0: phuong cua ban do, 0 là hướng bắc, 45 độ nghiêng
            //                                                                                       // DrawRoute3DInMap();

            if (myMap.Is3DSupported)
            {
                this.myMap.Style = MapStyle.Aerial3DWithRoads;

                BasicGeoposition spaceNeedlePosition = new BasicGeoposition();
                spaceNeedlePosition.Latitude = 47.6204;
                spaceNeedlePosition.Longitude = -122.3491;

                Geopoint spaceNeedlePoint = new Geopoint(spaceNeedlePosition);

                MapScene spaceNeedleScene = MapScene.CreateFromLocationAndRadius(spaceNeedlePoint,
                                                                                    400, /* show this many meters around */
                                                                                    0, /* looking at it to the south east*/
                                                                                    60 /* degrees pitch */);

                await myMap.TrySetSceneAsync(spaceNeedleScene);
            }
            else
            {
                //string status = "3D views are not supported on this device.";
                //rootPage.NotifyUser(status, NotifyType.ErrorMessage);
            }
        }
        bool Map3D = true;

        /// <summary>
        /// Show map 3d
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Map3D_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //showMap3D_Roads();
            //Route();
            int Donghieng = 0;
            if (Map3D)
            {
                Map3D = false;
                BtMap3D.Content = "ShowMap2D";
                Donghieng = 60;
            }
            else
            {
                BtMap3D.Content = "ShowMap3D";
                Map3D = true;
                Donghieng = 0;
                //myMap.Style = MapStyle.Road;
            }
            if (myMap.Is3DSupported)
            {
                this.myMap.Style = MapStyle.Road;

                BasicGeoposition spaceNeedlePosition = new BasicGeoposition();
                spaceNeedlePosition.Latitude = dLatDentination;
                spaceNeedlePosition.Longitude = dLonDentination;

                Geopoint spaceNeedlePoint = new Geopoint(spaceNeedlePosition);


                MapScene spaceNeedleScene = MapScene.CreateFromLocationAndRadius(spaceNeedlePoint,
                                                                                    400, /* show this many meters around */
                                                                                    0, /* looking at it to the south east*/
                                                                                    Donghieng /* degrees pitch */);

                await myMap.TrySetSceneAsync(spaceNeedleScene);
            }
            else
            {
                //string status = "3D views are not supported on this device.";
                //rootPage.NotifyUser(status, NotifyType.ErrorMessage);
            }


        }
        /*
        /// <summary>
        /// Không dùng trong project này, để phát triển mai sau
        /// </summary>
        //chi duong cua microsoft
        private async void Route()
        {
            // Start at Microsoft in Redmond, Washington.
            BasicGeoposition startLocation = new BasicGeoposition() { Latitude = 11.216412, Longitude = 106.957936 };

            // End at the city of Seattle, Washington.
            BasicGeoposition endLocation = new BasicGeoposition() { Latitude = 11.859860, Longitude = 106.92668 };

            // Get the route between the points.
            MapRouteFinderResult routeResult =
                  await MapRouteFinder.GetDrivingRouteAsync(
                  new Geopoint(startLocation),
                  new Geopoint(endLocation),
                  MapRouteOptimization.Time,
                  MapRouteRestrictions.None);

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                System.Text.StringBuilder routeInfo = new System.Text.StringBuilder();

                // Display summary info about the route.
                routeInfo.Append("Total estimated time (minutes) = ");
                routeInfo.Append(routeResult.Route.EstimatedDuration.TotalMinutes.ToString());
                routeInfo.Append("\nTotal length (kilometers) = ");
                routeInfo.Append((routeResult.Route.LengthInMeters / 1000).ToString());

                // Display the directions.
                routeInfo.Append("\n\nDIRECTIONS\n");

                foreach (MapRouteLeg leg in routeResult.Route.Legs)
                {
                    foreach (MapRouteManeuver maneuver in leg.Maneuvers)
                    {
                        routeInfo.AppendLine(maneuver.InstructionText);
                    }
                }

                // Load the textbox.
                //Hiện thông tin chỉ đường
                //tbOutputText.Text = routeInfo.ToString();
                //draw route
                // Use the route to initialize a MapRouteView.
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.
                myMap.Routes.Add(viewOfRoute);

                // Fit the MapControl to the route.
                await myMap.TrySetViewBoundsAsync(
                      routeResult.Route.BoundingBox,
                      null,
                      Windows.UI.Xaml.Controls.Maps.MapAnimationKind.None);
            }
            else
            {
                //tbOutputText.Text =
                      "A problem occurred: " + routeResult.Status.ToString();
            }
        }
        */
        private void Add_Line_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Map_DrawLine_2D(1, 1, 1, 1);
            Add_Icon();
            tb_ZoomLevel.Text = myMap.ZoomLevel.ToString();
        }
        //add icon to map
        void Add_Icon()
        {
            MapIcon icon = new MapIcon();
            Geopoint pointCenter = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude,
                Altitude = 200.0
            });
            Geopoint PositionOfPlane = pointCenter;
            Geopoint PositionIconOfPlane = new Geopoint(new BasicGeoposition()
            {
                Latitude = PositionOfPlane.Position.Latitude + 0.0001 * (21 - myMap.ZoomLevel),
                Longitude = PositionOfPlane.Position.Longitude - 0.0001 * (21 - myMap.ZoomLevel),//duong doc
                Altitude = 200.0
            });
            Geopoint PositionIconOfPlane1 = new Geopoint(new BasicGeoposition()
            {
                Latitude = PositionOfPlane.Position.Latitude + 0.00025 * (20 - myMap.ZoomLevel),
                Longitude = PositionOfPlane.Position.Longitude - 0.00025 * (20 - myMap.ZoomLevel),//duong doc
                Altitude = 200.0
            });
            if (myMap.ZoomLevel > 18)
            {
                icon.Location = PositionIconOfPlane;
            }
            else
            {
                if (myMap.ZoomLevel > 16.5)
                    icon.Location = PositionIconOfPlane1;
            }
            icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/airplane-icon.png"));

            icon.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;

            icon.Title = "My Plane";

            myMap.MapElements.Add(icon);


        }
        //Ngày 25/11/2015
        /*******Quay ảnh**********************/
        void rotateIcon()
        {
            MapIcon Original_icon = new MapIcon();
            Original_icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/airplane-icon.png"));
            BitmapIcon RotateBitmap = new BitmapIcon();
            //RotateBitmap.tr
            //Image Test = Original_icon.Image;
            Rectangle Test = new Rectangle();
            Test.Width = 100;
            Test.Height = 100;
            //Test.Fill = Colors.Green;
            myMap.Children.Add(Test);
            //Windows.Graphics.Imaging.BitmapRotation.None;
            //Image source = "airplane-icon.png";
            //RotateTransform
            //RotateTransform
        }
        private void RotateTransformSample()

        {

            Rectangle originalRectangle = new Rectangle();

            originalRectangle.Width = 200;

            originalRectangle.Height = 50;

            //originalRectangle.Fill = Brushes.Yellow;

            myMap.Children.Add(originalRectangle);



            Rectangle rotatedRectangle = new Rectangle();

            rotatedRectangle.Width = 200;

            rotatedRectangle.Height = 50;

            //rotatedRectangle.Fill = Brushes.Blue;

            rotatedRectangle.Opacity = 0.5;

            RotateTransform rotateTransform1 = new RotateTransform();

            rotatedRectangle.RenderTransform = rotateTransform1;

        }

        //myMap.Children.Add(rotatedRectangle);

        //Test**********************************
        private void Test(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {/*
            Image myImage = new Image();
            var stream = await image.OpenAsync(Windows.Storage.FileAccessMode.Read);
            var bitmapImage = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
            await bitmapImage.SetSourceAsync(stream);
            myImage.Source = bitmapImage;
            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
            */
        }
        /*
        void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawEllipse(155, 115, 80, 30, Colors.Black, 3);
            args.DrawingSession.DrawText("Hello, world!", 100, 100, Colors.Yellow);

        }
        */
        //ngày 28/11/2015 sử dụng Win2D.uwp
        private void TestWin2D()
        {
            //CanvasBitmap image = await CanvasBitmap.LoadAsync()
            /*
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("test.jpg");
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var device = new CanvasDevice();
                var bitmap = await CanvasBitmap.LoadAsync(device, stream);
                var renderer = new CanvasRenderTarget(device, bitmap.SizeInPixels.Width, bitmap.SizeInPixels.Height, bitmap.Dpi);

                using (var ds = renderer.CreateDrawingSession())
                {
                    var blur = new GaussianBlurEffect();
                    blur.BlurAmount = 8.0f;
                    blur.BorderMode = EffectBorderMode.Hard;
                    blur.Optimization = EffectOptimization.Quality;
                    blur.Source = bitmap;
                    ds.DrawImage(blur);
                }
                var saveFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("F:/Entertainment/Photo/picnic/temp.jpg", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                using (var outStream = await saveFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    await renderer.SaveAsync(outStream, CanvasBitmapFileFormat.Png);
                }
            }

            */
            //select file 
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add("F:/Entertainment/Photo/picnic/test.jpg");
            //picker.FileTypeFilter.Add(".jpeg");
            //picker.FileTypeFilter.Add(".png");
            //picker.PickSingleFileAndContinue();


            // create file  
            /*
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            savePicker.DefaultFileExtension = ".png";
            savePicker.SuggestedFileName = "resizedImage";
            savePicker.FileTypeChoices.Add("PNG", new string[] { ".png" });
            savePicker.PickSaveFileAndContinue();
            */



        }
        void OnDraw()
        {

        }
        //******************************************************************************
        //Test Emgu
        void LoadImage()
        {
            // Image<Bgr, Byte> img1 = new Image<Bgr, byte>("MyImage.jpg");
            //IntPtr inputImage = CvInvoke.cvLoadImage("C:\\Users\\...\\ClassPic1.jpg");
            // Mat img = new Mat(200, 420, DepthType.Cv8U, 3);
            //Mat img = new Mat();
            //Mat img1 = CvInvoke.Imread("E:/Studying/In School/hoc ki 7/Do An/2015/p4.jpg", Emgu.CV.CvEnum.LoadImageType.AnyColor);
            //add icon
            //img1.Save("E:/Studying/OutSchool/C shape Programming/code/DoAn_2015/Calculate Distance In Map 21_11_2015/CopyScenario2OfMapControl/Assets/test.png");
            BitmapImage tmp = new BitmapImage();
            //tmp.BeginInit();
            tmp.UriSource = new Uri("ms - appx:///Assets/airplane-icon.png", UriKind.Relative);
            tmp.DecodePixelWidth = 240;
            myMap.Children.Add(tmp);

            tmp = new BitmapImage(new Uri("ms-appx:///Assets/TechVista(300x300).png", UriKind.Absolute));
            //tmp.
            //this.bg.Source = tmp;

        }
        async System.Threading.Tasks.Task<byte[]> getBytesFromFileAsync(Windows.Storage.StorageFolder folder, string name)
        {
            //get from file
            var file = await folder.GetFileAsync(name);
            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
            return buffer.ToArray();
        }
        //Test Win 2D
        //Test Bitmap
        /// <summary>
        ///Load Image
        /// </summary>
        async void Image_Loaded()
        {
            //Image img = sender as Image;
            //BitmapImage bitmapImage = new BitmapImage();
            //img.Width = bitmapImage.DecodePixelWidth = 80; //natural px width of image source
            // don't need to set Height, system maintains aspect ratio, and calculates the other
            // dimension, so long as one dimension measurement is provided
            // bitmapImage.UriSource = new Uri(img.BaseUri, "F:/Entertainment/Photo/picnic/test.jpg");

            BitmapImage bitmapImage = new BitmapImage();
            Uri uri = new Uri("ms-appx:///Assets/plane.png");
            bitmapImage.UriSource = uri;

            // OR

            Image img = new Image();
            //img.Height = 50;
            //img.RenderTransform
            img.Opacity = 0.7;


            //img.Transitions.
            img.Source = new BitmapImage(new Uri("ms-appx:///Assets/airplane-icon.png"));

            img.RenderTransform = new RotateTransform()
            {
                Angle = 12.5,
                CenterX = 25, //The prop name maybe mistyped 
                CenterY = 25 //The prop name maybe mistyped 
            };

            myMap.Children.Add(img);
            //img.
            //Test WriteableBitmap
            // Initialize the WriteableBitmap with size 512x512 and set it as source of an Image control
            //WriteableBitmap writeableBmp = BitmapFactory.New(512, 512);

            //System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder.PngBitmapEncoder();
            //Create a new temporary image for saving
            //Image tempImage = new Image();

            //Create a new temporary image for saving

            //**************************************
            //Image<Bgr, Byte> frame = new Image<Bgr, Byte>(bit;

            // _capture.FlipHorizontal();
            //captureImageBox.Image = frame;
            //frame.Save(@"C:\Users\Hadi\Desktop\Camera Capture\MyImage.jpg");

            //var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("text.png");
            //var stream1 = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            Image tempImage = new Image();

            //Create a render object
            //System.IO.FileStream stream = new System.IO.FileStream("C:\newImage.png", FileMode.Create);
            // Save to a TGA image stream (file for example)
            //writeableBmp.WriteTga(stream);
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            //Render the app's display buffer
            await renderTargetBitmap.RenderAsync(img, (int)img.Width, (int)img.Height);
            //Set the temp image to the contents of the app's display buffer
            tempImage.Source = renderTargetBitmap;

            //Create a new file picker, set the default name, and extenstion
            Windows.Storage.Pickers.FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedFileName = "New LightTable.png";
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".png" });

            //Get the file the user selected
            Windows.Storage.StorageFile saveFile = await savePicker.PickSaveFileAsync();
            // Windows.Storage.StorageFile saveFile = await Windows.Storage.StorageFile.GetFileFromPathAsync("F:/Entertainment/Photo/picnic/test.jpg");
            //Only move on if the user actually selected a file
            if (saveFile != null)
            {
                //Get a buffer of the pixels captured from the screen
                Windows.Storage.Streams.IBuffer buffer = await renderTargetBitmap.GetPixelsAsync();

                //Get a stream of the data in the buffer
                System.IO.Stream stream = buffer.AsStream();

                //Convert the stream into a IRandomAccessStream because I don't know what I'm doing.
                Windows.Storage.Streams.IRandomAccessStream raStream = stream.AsRandomAccessStream();

                //Attempt to encode the stream into a PNG
                Windows.Graphics.Imaging.BitmapEncoder encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, raStream);

                //Get a stream for the file the user selected
                Windows.Storage.Streams.IRandomAccessStream fileStream = await saveFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                //FIND SOME WAY TO SAVE raStream TO fileStream
                //Something like:
                // await fileStream.WriteAsync(raStream.AsStreamForRead());
                //Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
                //new Uri("ms-appx:///Assets/image.png"));

                //await file.CopyAsync(Windows.Storage.ApplicationData.Current.LocalFolder, "image.png");
            }
            //WriteableBitmap bmp = new WriteableBitmap()

            //img = 0.5 * img;
            //bitmapImage = 2 * bitmapImage;
            //or
            /*
            Image img = sender as Image;
            if (img != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                img.Width = bitmapImage.DecodePixelWidth = 280;
                bitmapImage.UriSource = new Uri("ms-appx:///Assets/plane.png");
                img.Source = bitmapImage;
            }
            */

            //Test Icon
            MapIcon icon = new MapIcon();
            Geopoint pointCenter = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude,
                Altitude = 200.0
            });
            Geopoint PositionOfPlane = pointCenter;
            Geopoint PositionIconOfPlane = new Geopoint(new BasicGeoposition()
            {
                Latitude = PositionOfPlane.Position.Latitude + 0.0001 * (21 - myMap.ZoomLevel),
                Longitude = PositionOfPlane.Position.Longitude - 0.0001 * (21 - myMap.ZoomLevel),//duong doc
                Altitude = 200.0
            });
            Geopoint PositionIconOfPlane1 = new Geopoint(new BasicGeoposition()
            {
                Latitude = PositionOfPlane.Position.Latitude + 0.00025 * (20 - myMap.ZoomLevel),
                Longitude = PositionOfPlane.Position.Longitude - 0.00025 * (20 - myMap.ZoomLevel),//duong doc
                Altitude = 200.0
            });
            if (myMap.ZoomLevel > 18)
            {
                icon.Location = PositionIconOfPlane;
            }
            else
            {
                if (myMap.ZoomLevel > 16.5)
                    icon.Location = PositionIconOfPlane1;
            }
            //icon.Image = RandomAccessStreamReference.CreateFromFile(img);

            icon.CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible;

            icon.Title = "My Plane";

            myMap.MapElements.Add(icon);

            //******************************************
            //saveFile
            Windows.Storage.StorageFile placeholderImage = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("Assets\\airplane-icon.png");
            //Rotate
            /*
            BitmapImage myBitmapImage = new BitmapImage();

            myBitmapImage.UriSource = new Uri(@"C:\Documents and Settings\All Users\Documents\My Pictures\Sample Pictures\Water Lilies.jpg");

            myBitmapImage.DecodePixelWidth = 200;
            */
            /*
            Image image = new Image()
            {
                Height = 50,
                Width = 50,
                RenderTransform = new RotateTransform()
                {
                    Angle = 90,
                    CenterX = 25, //The prop name maybe mistyped 
                    CenterY = 25 //The prop name maybe mistyped 
                },
                Source = new BitmapImage(new Uri("ms - appx:///Assets/airplane-icon.png"))
            };
            */

            //myMap.Children.Add(image);


            //cv::imwrite(localFile, image);
            //***************************************************************
            var image = new WriteableBitmap(50, 50);

            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/airplane-icon.png"));

            var content = await file.OpenReadAsync();

            image.SetSource(content);
            await SaveImage((WriteableBitmap)img.Source);




        }

        //********************************************************
        private async Task LoadImage1()
        {
            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("text.png");
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);
            //image.Source = bitmapImage;
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //Microsoft.Graphics graphics = Microsoft.Graphics.FromImage(map);
            //Microsoft.Graphics.Canvas.UI.Xaml.
            //var encoder = new PngBitmapEncoder();

        }

        public async void IsolatedStorage()
        {
            // settings
            var _Name = "MyFileName";
            var _Folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var _Option = Windows.Storage.CreationCollisionOption.ReplaceExisting;

            // create file 
            var _File = await _Folder.CreateFileAsync(_Name, _Option);
            //Assert.IsNotNull(_File, "Create file");

            // write content



        }
        //Ngay 1/12/2015 Test Emgu CV
        void EmguCV()
        {
            //WImage image = new WImage();

        }

        //**************************************************
        async Task SaveImage(WriteableBitmap saveImage)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPG File", new List<string>() {
                 ".jpg"
             });
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(
                        BitmapEncoder.JpegEncoderId, stream);
                    Stream pixelStream = saveImage.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);


                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                        (uint)saveImage.PixelWidth, (uint)saveImage.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();
                }
            }
        }
        async Task saveBytesToFileAsync(StorageFolder folder, string filename, byte[] bytes)
        {
            Image img1 = new Image();
            var file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, bytes);
        }


        //Reading text from a file
        public static async Task<string> readStringFromLocalFile(string filename)
        {
            // reads the contents of file 'filename' in the app's local storage folder and returns it as a string

            // access the local folder
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            // open the file 'filename' for reading
            Stream stream = await local.OpenStreamForReadAsync(filename);
            string text;

            // copy the file contents into the string 'text'
            using (StreamReader reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
        async void Test()
        {
            string testData;
            testData = await readStringFromLocalFile("OnNghia.txt");
            await saveStringToLocalFile("OnNghia.txt", "PhamCongDuc");
            // Image byte[] = new byte();
            //Image i = Image.fr
        }
        //Write text to a file
        async Task saveStringToLocalFile(string filename, string content)
        {
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        //Save Image Byte[] To File
        async Task TestsaveBytesToFileAsync(StorageFolder folder, string filename, byte[] bytes)
        {
            var file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, bytes);
        }





        //**********************Timer************************
        //**************************************************
        private DispatcherTimer timer, TimerReadData;
        //Int32 TestTimer = 0;



        //Tạo một mảng để lưu các Acc từ hàng 1 đến hàng 10
        //Biến này lưu Data Acc thành mảng từ DataAcc[0] đến DataAcc[9]
        DataFromSensor Data = new DataFromSensor();
        int index_dataAcc = 0;
        //Chú ý muốn nhận cổng com của cường thì phải cài driver cho nó
        //Ngày 03/12/2015 22h27 đã đọc xong data
        /// <summary>
        /// Ngắt timer để đọc data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// ngày 12/12/2015 đọc tất cả các gia tốc dựa vào ký tự '\n' hoặc LR CR 10,13 mã nhị phân
        /// Đọc chuỗi data nếu thấy \n thì lưu vào mãng, sau đó phân tích
        /// 
        public void TimerReadData_Tick(object sender, object e)
        {
            //Nếu đã connect được UART thì bắt đầu đọc data
            /*
            -0011  0084 -0040 -0003  0012 -0007 -0143 -0016  0976  2687  0175  2041  0849 
            $GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
            $GPVTG,110.33,T,,M,0.18,N,0.33,K,A*34
            */
            if (bConnectOk)
            {
                /*
                //Tìm gia tốc bằng cách phát hiện chuỗi $GPGGA
                DataFromSensor Data = new DataFromSensor();
                if (strDataFromSerialPort.IndexOf("$GPGGA") != -1)
                {
                    Data.Temp = FindStrGPGGA(strDataFromSerialPort);
                    if(Data.Temp.IndexOf('$') >= 80)
                    //tách lấy 1 giá trị gia tốc trong chuỗi 10 giá trị gia tốc thu được trong 100ms
                    Data.Acc = Data.Temp.Substring(Data.Temp.IndexOf('$') - 80, 80);
                    //tbOutputText.Text += "Temp0: " + Data.Temp + '\n';
                    //tbOutputText.Text += "Acc: " + Data.Acc + '\n';
                }
                //Tìm vĩ độ, kinh độ và độ cao bằng cách tìm chuỗi $GPVTG
                if (strDataFromSerialPort.IndexOf("$GPVTG") != -1)
                {
                    Data.Temp = FindTextInStr(strDataFromSerialPort, "$GPVTG");
                    //tbOutputText.Text += "Temp1: " + Data.Temp + '\n';
                    //Tách lấy data Time, lat, long, alt
                    if (Data.Temp.Length >= 60)
                    {
                        //024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
                        //tách lấy giờ
                        Data.Time = Data.Temp.Substring(Data.Temp.IndexOf("GA") + 1, 10);
                        //tbOutputText.Text += "Time: " + Data.Time + '\n';
                        //tách lấy vĩ độ
                        Data.Latitude = Data.Temp.Substring(Data.Temp.IndexOf("GA") + 12, 9);
                        //tbOutputText.Text += "Latitude: " + Data.Latitude + '\n';
                        //tách lấy kinh độ
                        Data.Longtitude = Data.Temp.Substring(Data.Temp.IndexOf("GA") + 24, 10);
                        //tbOutputText.Text += "Longtitude: " + Data.Longtitude + '\n';
                        //tách lấy độ cao so với mực nước biển
                        //tìm ký tự M đầu tiên trong chuỗi
                        //Temp =  $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9,M,2.5,M,,*6E
                        Data.Temp = Data.Temp.Substring(0, Data.Temp.IndexOf('M') - 1);
                        //Temp = $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9
                        //Độ cao là số sao dấu phẩy cuối cùng
                        Data.Altitude = Data.Temp.Substring(Data.Temp.LastIndexOf(',') + 1);
                        //tbOutputText.Text += "Altitude: " + Data.Altitude + '\n';
                    }
                }
                //Tìm góc của sensor
                if (strDataFromSerialPort.IndexOf(",K,A") != -1)
                {
                    //$GPVTG,350.40,T,,M,0.95,N,1.76,K,A
                    Data.Temp = FindTextInStr(strDataFromSerialPort, ",K,A");
                    //Temp = $GPVTG,350.40,T,,M,0.95,N,1.76,K,A
                    //tbOutputText.Text += "Temp2: " + Data.Temp + '\n';
                    //tách lấy góc
                    Data.Angle = Data.Temp.Substring(Data.Temp.IndexOf("TG") + 3, 6);
                    //tbOutputText.Text += "Angle: " + Data.Angle + '\n';
                }
                */
                //ngày 12/12/2015 đọc dựa vào ký tự Enter
                //Nếu đã connect được UART thì bắt đầu đọc data
                /*
                -0011  0084 -0040 -0003  0012 -0007 -0143 -0016  0976  2687  0175  2041  0849 
                $GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
                $GPVTG,110.33,T,,M,0.18,N,0.33,K,A*34
                */
                //Tìm gia tốc bằng cách phát hiện chuỗi $GPGGA

                //Test data thực tế với code sau
                //if (strDataFromSerialPort.IndexOf(Convert.ToChar(10).ToString() + Convert.ToChar(13).ToString()) != -1)
                //***********************
                //Test data lan 1 vơi code sau và thực tế với code sau
                if (strDataFromSerialPort.IndexOf('\n') != -1)
                //************************************
                {
                    //Data.Temp = FindTextInStr(strDataFromSerialPort, '\n');
                    //hàng từ 0 đến 9 là của Acc
                    //Acc gồm 10 hàng
                    //trong data cái đầu tiên chỉ có 9 hàng nên ta thêm nếu kí tự đầu tiên của chuỗi khác $ thì đó mới là Acc
                    if ((Data.Temp[0] == ' ') && Data.Temp.Length > 2 && (Data.Temp[0] != '-')) //Do có lúc chỉ có 1 hàng trắng
                    {
                        //Mặc định không lưu dữ liệu không hiểu thị các giá trị ra textbox
                        //-0011  0084 - 0040 - 0003  0012 - 0007 - 0143 - 0016  0976  2687  0175  2041  0849

                        Data.DataAcc = Data.Temp;
                        //Lấy Roll, mỗi giá trị góc dài 6 ký tự
                        //Data của Anh Bình có xuất hiện chỗ trục trặt nôi chặn lỗi này
                        //
                        //GNSS OTP:  GPS GLO, SEL:  GPS GLO*79 G
                        //$GPTXT,01,01,02,ANTSUPERV = AC SD PDoS SR * 20 G
                        //$GPTXT,01,01,02,ANTSTATUS = DONTKNOW * 33 G
                        //$GPTXT,01,01,02,FIS 0xEF4015(79189) found * 33 G
                        //$GPTXT,01,01,02,LLC FFFFFFFF-FFFFFFED - FFFFFFFF - FFFFFFFF - FFFFFF69 * 20 G
                        //$GPTXT,01,01,02,RF0 dev ok * 1A G
                        //bằng cách thêm dòng (Data.Temp[0] != 'G')
                        //Thêm lỗi này
                        //1,01,02,ANTSTATUS = DONTKNOW * 33 G
                        //$GPTXT,01,01,02,FIS 0xEF4015(79189) found * 33 G
                        //$GPTXT,01,01,02,LLC FFFFFFFF-FFFFFFED - FFFFFFFF - FFFFFFFF - FFFFFF69 * 20 G
                        //$GPTXT,01,01,02,RF0 dev ok * 1A G
                        Data.Roll = Data.DataAcc.Substring(0, 6);
                        Data.Pitch = Data.DataAcc.Substring(6, 6);
                        Data.Yaw = Data.DataAcc.Substring(12, 6);

                        ////tbOutputText.Text += "ACC[" + (index_dataAcc).ToString() + "]: " + Data.DataAcc[index_dataAcc] + '\n';
                        //index_dataAcc++;
                    }

                    //if (Data.Temp.IndexOf('$') >= 80)
                    //tách lấy 1 giá trị gia tốc trong chuỗi 10 giá trị gia tốc thu được trong 100ms
                    //Data.Acc = Data.Temp.Substring(Data.Temp.IndexOf('$') - 80, 80);
                    ////tbOutputText.Text += "Temp: " + Data.Temp + '\n';
                    //if(index_dataAcc <= 10 && (index_dataAcc > 0))

                    //Tìm chuối bắt đầu với $GPGGA để tìm lat long, alt
                    //Data.Temp = "$GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                    if (Data.Temp.IndexOf("$GPG") != -1)
                    {
                        //Chuỗi này chứa lat, long, alt
                        //$GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
                        //Save Data to txt

                        //


                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy giờ
                        Data.Time = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //Ngày 17/12/2015 17h36 ok
                        //tbOutputText.Text += "Time: " + Data.Time + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy vĩ độ mặc định ở vĩ độ North
                        Data.Latitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Latitude: " + Data.Latitude + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //tách lấy kinh độ
                        Data.Longtitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Longtitude: " + Data.Longtitude + '\n';
                        //tách lấy độ cao so với mực nước biển
                        //tìm ký tự M đầu tiên trong chuỗi
                        //Temp =  $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9,M,2.5,M,,*6E
                        if (Data.Temp.IndexOf('M') > 0)
                        {
                            Data.Temp = Data.Temp.Substring(0, Data.Temp.IndexOf('M') - 1);
                            //Temp = $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9
                            //Độ cao là số sao dấu phẩy cuối cùng
                            Data.Altitude = Data.Temp.Substring(Data.Temp.LastIndexOf(',') + 1);
                        }
                        //tbOutputText.Text += "Altitude: " + Data.Altitude + '\n';
                        //*************************************************************************************



                        //Reset bộ đếm số dòng của cảm biến IMU
                        //index_dataAcc = 0;

                    }
                    //Tìm chuối bắt đầu với $GPVTG để tìm angle
                    if (Data.Temp.IndexOf("$GPV") != -1)
                    {
                        //Chuỗi này chứa angle
                        //Reset bộ đếm số dòng của cảm biến IMU
                        //index_dataAcc = 0;
                        //$GPVTG,350.40,T,,M,0.95,N,1.76,K,A

                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "350.40,T,,M,0.95,N,1.76,K,A";
                        //tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy góc
                        Data.Angle = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Angle: " + Data.Angle + '\n';
                        //Tách vận tốc
                        //cut bỏ Data đến chữ N và dấu phẩy kế chữ N nên mới +2 lấy sau dấu phẩy
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf('N') + 2, Data.Temp.Length - (Data.Temp.IndexOf('N') + 2));
                        //tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //Data.Temp = "1.76,K,A";
                        //tách lấy speed km/h
                        //De phong khong co dau ,
                        if (Data.Temp.IndexOf(',') != -1)
                            Data.Speed = Data.Temp.Substring(0, Data.Temp.IndexOf(','));

                        //tbOutputText.Text += "Speed: " + Data.Speed + '\n';
                    }
                }
            }
        }
        //******************************************************************************
        //xử lý data không dùng timer
        public void ProcessData()
        {
            //Nếu đã connect được UART thì bắt đầu đọc data
            /*
            -0011  0084 -0040 -0003  0012 -0007 -0143 -0016  0976  2687  0175  2041  0849 
            $GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
            $GPVTG,110.33,T,,M,0.18,N,0.33,K,A*34
            */
            if (bConnectOk)
            {


                //**************************************************************************
                //Format mới cuả data ngày 03/03/2016
                /*
                $  0006 -0003  1594  0006  0000 -0006  0016  0003  0986 -0161 -0058  0130  1078 
                $GPVTG,,T,,M,0.209,N,0.387,K,D*21
                $GPGGA,063621.90,1045.56915,N,10639.72723,E,2,10,1.93,37.0,M,-2.5,M,,0000*72
                $  0006 -0002  1595  0021  0006 -0021  0016  0000  0989 -0160 -0057  0128  1076 
                $GPVTG,,T,,M,0.129,N,0.238,K,D*25
                $GPGGA,063622.10,1045.56914,N,10639.72725,E,2,10,1.93,37.0,M,-2.5,M,,0000*7E
                */
                //Nên ta có code sau
                //Block new code***********************************************

                if (strDataFromSerialPort.IndexOf("\r\n") != -1)//Bắt ký tự $
                //************************************
                {

                    Data.Temp = FindTextInStr(strDataFromSerialPort, "\r\n");
                    if (Data.Temp.IndexOf('$') != -1)
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf('$') + 1, Data.Temp.Length - (Data.Temp.IndexOf('$') + 1));
                    else
                        return;
                    //hàng từ 0 đến 9 là của Acc
                    //Acc gồm 10 hàng
                    //trong data cái đầu tiên chỉ có 9 hàng nên ta thêm nếu kí tự đầu tiên của chuỗi khác $ thì đó mới là Acc
                    if ((Data.Temp[0] != 'G') && (Data.Temp.Length >= 18)) //Do có lúc chỉ có 1 hàng trắng
                    {
                        //Mặc định không lưu dữ liệu không hiểu thị các giá trị ra textbox
                        //-0011  0084 - 0040 - 0003  0012 - 0007 - 0143 - 0016  0976  2687  0175  2041  0849

                        Data.DataAcc = Data.Temp;
                        //Lấy Roll, mỗi giá trị góc dài 6 ký tự
                        //Data của Anh Bình có xuất hiện chỗ trục trặt nôi chặn lỗi này
                        //
                        //GNSS OTP:  GPS GLO, SEL:  GPS GLO*79 G
                        //$GPTXT,01,01,02,ANTSUPERV = AC SD PDoS SR * 20 G
                        //$GPTXT,01,01,02,ANTSTATUS = DONTKNOW * 33 G
                        //$GPTXT,01,01,02,FIS 0xEF4015(79189) found * 33 G
                        //$GPTXT,01,01,02,LLC FFFFFFFF-FFFFFFED - FFFFFFFF - FFFFFFFF - FFFFFF69 * 20 G
                        //$GPTXT,01,01,02,RF0 dev ok * 1A G
                        //bằng cách thêm dòng (Data.Temp[0] != 'G')
                        //Thêm lỗi này
                        //1,01,02,ANTSTATUS = DONTKNOW * 33 G
                        //$GPTXT,01,01,02,FIS 0xEF4015(79189) found * 33 G
                        //$GPTXT,01,01,02,LLC FFFFFFFF-FFFFFFED - FFFFFFFF - FFFFFFFF - FFFFFF69 * 20 G
                        //$GPTXT,01,01,02,RF0 dev ok * 1A G
                        Data.Roll = Data.DataAcc.Substring(0, 6);
                        Data.Pitch = Data.DataAcc.Substring(6, 6);
                        Data.Yaw = Data.DataAcc.Substring(12, 6);


                        //vẽ luôn
                        if (bSetup)
                        {
                            //dùng các biến tạm để kiểm tra có convert được hay không
                            double temp_Roll = 0, temp_Pitch = 0, temp_Raw = 0;
                            try
                            {
                                temp_Roll = Convert.ToDouble(Data.Roll) / 10;
                                temp_Pitch = Convert.ToDouble(Data.Pitch) / 10;
                                temp_Raw = Convert.ToDouble(Data.Yaw) / 10;
                            }
                            catch
                            {

                            }

                            //Vẽ sự thay đổi PitchAndRoll_Draw đã tối ưu ngày 3,4 /03/2016
                            PitchAndRoll_Draw(temp_Roll, temp_Pitch, 350 + i16EditPosition * 11 / 6, 200, 140, 50);
                            //ComPass_Draw_Compass_optimize (Convert.ToDouble(Data.Yaw) / 10, 350 + i16EditPosition  * 11 / 6, 500, 120);
                            //ComPass_Draw_Compass(Convert.ToDouble(Data.Yaw) / 10, 350 + i16EditPosition  * 11 / 6, 500, 120);
                            //Chỉ quay máy bay và thay đổi góc yaw để tối ưu
                            //Comp_RotateAndAddValue(Convert.ToDouble(Data.Yaw) / 10);
                            //Comp_RotateAndAddValue(Math.Round(257.9,0));
                            Comp_Rotate_OutAndAddValue(temp_Raw);

                        }

                        ////tbOutputText.Text += "ACC[" + (index_dataAcc).ToString() + "]: " + Data.DataAcc[index_dataAcc] + '\n';
                        //index_dataAcc++;
                    }

                    //if (Data.Temp.IndexOf('$') >= 80)
                    //tách lấy 1 giá trị gia tốc trong chuỗi 10 giá trị gia tốc thu được trong 100ms
                    //Data.Acc = Data.Temp.Substring(Data.Temp.IndexOf('$') - 80, 80);
                    ////tbOutputText.Text += "Temp: " + Data.Temp + '\n';
                    //if(index_dataAcc <= 10 && (index_dataAcc > 0))

                    //Tìm chuối bắt đầu với $GPGGA để tìm lat long, alt
                    //Data.Temp = "$GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                    if (Data.Temp.IndexOf("GPG") != -1)
                    {
                        //Chuỗi này chứa lat, long, alt
                        //$GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
                        //Save Data to txt

                        //


                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy giờ, thời gian GPS chậm hơn thời gian thực 7h nên phải cộng 7
                        //Nếu time >= 240000.00 thì phải trừ đi 240000.00
                        double dTemp_Time = 0;
                        string temp_time = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        if (temp_time != "")
                        {
                            dTemp_Time = (Convert.ToDouble(temp_time) + 70000.00);
                            if (dTemp_Time >= 240000.00) dTemp_Time -= 240000.00;
                            Data.Time = dTemp_Time.ToString();
                        }
                        //Ngày 17/12/2015 17h36 ok
                        //tbOutputText.Text += "Time: " + Data.Time + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy vĩ độ mặc định ở vĩ độ North
                        Data.Latitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Latitude: " + Data.Latitude + '\n';
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                        //tách lấy kinh độ
                        Data.Longtitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Longtitude: " + Data.Longtitude + '\n';
                        //tách lấy độ cao so với mực nước biển
                        //tìm ký tự M đầu tiên trong chuỗi
                        //Temp =  $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9,M,2.5,M,,*6E
                        if (Data.Temp.IndexOf('M') > 0)
                        {
                            Data.Temp = Data.Temp.Substring(0, Data.Temp.IndexOf('M') - 1);
                            //Temp = $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9
                            //Độ cao là số sao dấu phẩy cuối cùng
                            Data.Altitude = Data.Temp.Substring(Data.Temp.LastIndexOf(',') + 1);
                        }
                        //tbOutputText.Text += "Altitude: " + Data.Altitude + '\n';
                        //*************************************************************************************



                        //Reset bộ đếm số dòng của cảm biến IMU
                        //index_dataAcc = 0;

                    }
                    //Tìm chuối bắt đầu với $GPVTG để tìm angle
                    if (Data.Temp.IndexOf("GPV") != -1)
                    {
                        //Chuỗi này chứa angle
                        //Reset bộ đếm số dòng của cảm biến IMU
                        //index_dataAcc = 0;
                        //$GPVTG,350.40,T,,M,0.95,N,1.76,K,A

                        //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                        //Data.Temp = "350.40,T,,M,0.95,N,1.76,K,A";
                        //tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //tách lấy góc
                        Data.Angle = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                        //tbOutputText.Text += "Angle: " + Data.Angle + '\n';
                        //Tách vận tốc
                        //cut bỏ Data đến chữ N và dấu phẩy kế chữ N nên mới +2 lấy sau dấu phẩy
                        Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf('N') + 2, Data.Temp.Length - (Data.Temp.IndexOf('N') + 2));
                        //tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                        //Data.Temp = "1.76,K,A";
                        //tách lấy speed km/h
                        //De phong khong co dau ,
                        if (Data.Temp.IndexOf(',') != -1)
                            Data.Speed = Data.Temp.Substring(0, Data.Temp.IndexOf(','));

                        //tbOutputText.Text += "Speed: " + Data.Speed + '\n';
                    }
                    //End of Block new code***********************************************
                }
            }
        }
        //******************************************************************************

        //double index_Lat = 10.818345, index_Lon = 106.658897, Timer_fly = 0;
        /// <summary>
        /// 03/12/2015
        /// Muc tiêu: trong 100ms phải đọc được 
        /// 1 gia tốc sensor.Acc
        /// 1 thời gian sensor.Time
        /// 1 ....
        /// timer chu kỳ là 100ms
        /// Vẽ Quỹ đạo máy bay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TimerShowData(object sender, object e)
        {
            //TestTimer ++;
            double temp_angle;


            try
            {
                if (Data.Latitude != null)
                {
                    //*************************************************************************************
                    //Có Data mới nên vẽ vị trí mới máy bay
                    //Có giải thích trong file word ngày 31/12/2015
                    double DoLat, DoLon;
                    if ((Data.Latitude.IndexOf('.') - 2) >= 0)
                    {
                        DoLat = Convert.ToDouble(Data.Latitude.Substring(0, Data.Latitude.IndexOf('.') - 2));

                        DoLon = Convert.ToDouble(Data.Longtitude.Substring(0, Data.Longtitude.IndexOf('.') - 2));
                        //dLatFinal = DoLat + Convert.ToDouble(Data.Latitude.Substring(2, Data.Latitude.Length - 2)) / 60;

                        //Ngay 13/01/2016
                        //Chú ý khi Angle = null thì Convert.ToDouble(Data.Angle) tính không được
                        //tính toán giá trị Lat, lon lưu vào biến toàn cục
                        dLatGol = DoLat + Convert.ToDouble(Data.Latitude.Substring(2, Data.Latitude.Length - 2)) / 60;
                        dLonGol = DoLon + Convert.ToDouble(Data.Longtitude.Substring(Data.Longtitude.IndexOf('.') - 2, Data.Longtitude.Length -
                                        (Data.Longtitude.IndexOf('.') - 2))) / 60;
                    }
                    if ((Data.Angle != "") && (Data.Angle != null))
                    {
                        //Draw_Trajectory_And_Flight(dLatGol, dLonGol,
                        //            Convert.ToDouble(Data.Altitude), Convert.ToDouble(Data.Angle));
                        Draw_Trajectory_And_Flight_optimize(dLatGol, dLonGol,
                                        Convert.ToDouble(Data.Altitude), Convert.ToDouble(Data.Angle));
                    }
                    else
                        //Draw_Trajectory_And_Flight(dLatGol, dLonGol,
                        //                Convert.ToDouble(Data.Altitude), 0.0);
                        Draw_Trajectory_And_Flight_optimize(dLatGol, dLonGol,
                                        Convert.ToDouble(Data.Altitude), 0.0);

                    //**********************************************************************
                    //Ngày 20/01/2016
                    //caculate distance
                    //Tan Son Nhat Airport dLatDentination, dLonDentination
                    //Point2: 10.113574, 106.052579
                    dDistanToTaget = distance(dLatGol, dLonGol, Convert.ToDouble(Data.Altitude), dLatDentination, dLonDentination, 0);
                    temp_angle = angleFromCoordinate(dLatGol, dLonGol, dLatDentination, dLonDentination);
                    //Ta hiện khoảng cách trên đường thẳng chứ không hiện textbox nên bỏ dòng sau
                    //tbShowDis.Text = "Distance to Dentination:  " + dDistanToTaget.ToString() + "\n";
                    //Tinh goc giua 2 diem từ vị trí máy bay đến đích
                    tbShowDis.Text = "Vector Angle:  " + temp_angle.ToString();

                    //Ngay 21/1/2016 Show Data
                    //ShowDistance(0, temp_angle + 90, dDistanToTaget.ToString() + " Meter", 30 * myMap.ZoomLevel / 22, dLatGol, dLonGol, 1);//Purple
                    //**********optimize 6/3/2016
                    ShowDistance_optimize(0, temp_angle + 90, dDistanToTaget.ToString() + " Meter", 50 * myMap.ZoomLevel / 22, dLatGol, dLonGol);//Purple

                }
                if (bSetup)
                {
                    //Neu angle la null thi convert k duoc
                    if (Data.Angle != "")
                        DisplayDataOnMap(Convert.ToDouble(Data.Roll) / 10, Convert.ToDouble(Data.Pitch) / 10, Convert.ToDouble(Data.Speed),
                            Convert.ToDouble(Data.Altitude), 0, Convert.ToDouble(Data.Angle));
                    else
                        DisplayDataOnMap(Convert.ToDouble(Data.Roll) / 10, Convert.ToDouble(Data.Pitch) / 10, Convert.ToDouble(Data.Speed),
                            Convert.ToDouble(Data.Altitude), 0, 0.0);

                }
            }
            catch
            {

            }




        }
        //************************************************************************
        /// <summary>
        /// Timer chỉ hiện thị gia tốc
        /// góc roll là mũi tên chạy
        /// góc pitch là các đường chạy lên xuống
        /// Theo trục 0xyz đã format ngày 03/03/2016 thì dữ liệu thu được 6 ký tự roll, 6 ký tự pitch, 6 ký tự yaw
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Timer_ShowAcc_Tick(object sender, object e)
        {
            if (bSetup)
            {
                //dùng các biến tạm để kiểm tra có convert được hay không
                double temp_Roll = 0, temp_Pitch = 0, temp_Raw = 0;
                try
                {
                    temp_Roll = Convert.ToDouble(Data.Roll) / 10;
                    temp_Pitch = Convert.ToDouble(Data.Pitch) / 10;
                    temp_Raw = Convert.ToDouble(Data.Yaw) / 10;
                }
                catch
                {

                }

                //Vẽ sự thay đổi PitchAndRoll_Draw đã tối ưu ngày 3,4 /03/2016
                PitchAndRoll_Draw(temp_Roll, temp_Pitch, 350 + i16EditPosition * 11 / 6, 200, 140, 50);
                //ComPass_Draw_Compass_optimize (Convert.ToDouble(Data.Yaw) / 10, 350 + i16EditPosition  * 11 / 6, 500, 120);
                //ComPass_Draw_Compass(Convert.ToDouble(Data.Yaw) / 10, 350 + i16EditPosition  * 11 / 6, 500, 120);
                //Chỉ quay máy bay và thay đổi góc yaw để tối ưu
                //Comp_RotateAndAddValue(Convert.ToDouble(Data.Yaw) / 10);
                //Comp_RotateAndAddValue(Math.Round(257.9,0));
                Comp_Rotate_OutAndAddValue(temp_Raw);

            }
        }

        //******************************************************
        public void ShowAcc()
        {
            if (bSetup)
            {
                //Vẽ sự thay đổi
                PitchAndRoll_Draw(Convert.ToDouble(Data.Roll) / 10, Convert.ToDouble(Data.Pitch) / 10, 350 + i16EditPosition  * 11 / 6, 200, 140, 50);

            }
        }

        //Doc tu dau den ký tự c trong chuỗi sau đó tạo ra chuỗi mới loại bỏ phần đã được lấy
        //Var in oder to Receive Data from Sensor


        /// <summary>
        /// Đọc data đến ký tự c và giữa luôn ký tự c lại
        /// </summary>
        /// <param name="stIsProcess"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public string ReadToChar(string stIsProcess, char c)
        {
            int index = 0;
            char[] b = new char[stIsProcess.Length];
            string ReturnData = "";
            ReturnData += stIsProcess[index];
            if (stIsProcess.IndexOf(c) != -1)//Có ký tự trong chuỗi
            {
                while (stIsProcess[index] != c)
                {
                    //ReturnData[index] = '0';

                    index++;
                    ReturnData += stIsProcess[index];
                    //b[index] = '0';
                    //stIsProcess.Substring(0, 2) = "q";
                    //StringReader sr = new StringReader(ch);
                    //sr.Read()
                }
                //Loai bo ky tu 
                /*
                Ví dụ
                stIsProcess = "doan1"
                c = a
                i = 0
                return = d
                i = 1
                return = do
                i = 2

                cần cắt doa nên ta gọi stDataFromSerialPort.Remove(0, 3)
                3 ký tự kể từ ký tự đầu tiên
                */
                strDataFromSerialPort = stIsProcess.Remove(0, index);
            }
            return ReturnData;
        }
        //Doc khong cut ki tu
        public string ReadToCharNoCut(string stIsProcess, char c)
        {
            int index = 0;
            char[] b = new char[stIsProcess.Length];
            string ReturnData = "";
            ReturnData += stIsProcess[index];
            if (stIsProcess.IndexOf(c) != -1)//Có ký tự trong chuỗi
            {
                while (stIsProcess[index] != c)
                {
                    //ReturnData[index] = '0';

                    index++;
                    ReturnData += stIsProcess[index];
                    //b[index] = '0';
                    //stIsProcess.Substring(0, 2) = "q";
                    //StringReader sr = new StringReader(ch);
                    //sr.Read()
                }
                //Loai bo ky tu 
                /*
                Ví dụ
                stIsProcess = "doan1"
                c = a
                i = 0
                return = d
                i = 1
                return = do
                i = 2

                cần cắt doa nên ta gọi stDataFromSerialPort.Remove(0, 3)
                3 ký tự kể từ ký tự đầu tiên
                */
                //stDataFromSerialPort = stIsProcess.Remove(0, index);
            }
            return ReturnData;
        }
        public string ReadAltitude(string stIsProcess, char c)
        {
            int index = 0;
            char[] b = new char[stIsProcess.Length];
            string ReturnData = "";
            //ReturnData += stIsProcess[index];
            if (stIsProcess.IndexOf(c) != -1)//Có ký tự trong chuỗi
            {
                while (stIsProcess[index] != c)
                {
                    //ReturnData[index] = '0';


                    ReturnData += stIsProcess[index];
                    index++;
                    //b[index] = '0';
                    //stIsProcess.Substring(0, 2) = "q";
                    //StringReader sr = new StringReader(ch);
                    //sr.Read()
                }
                //Loai bo ky tu 
                /*
                Ví dụ
                stIsProcess = "doan1"
                c = a
                i = 0
                return = d
                i = 1
                return = do
                i = 2

                cần cắt doa nên ta gọi stDataFromSerialPort.Remove(0, 3)
                3 ký tự kể từ ký tự đầu tiên
                */
                ReturnData = stIsProcess.Remove(0, index + 1);
            }
            return ReturnData;
        }
        /// <summary>
        /// Trả về chuỗi trước ký tự c
        /// Cut các ký tự đó kèm ký tự c trong chuỗi stDataFromSerialPort
        /// </summary>
        /// <param name="stIsProcess"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public string CutChar(string stIsProcess, char c)
        {
            int index = 0;
            char[] b = new char[stIsProcess.Length];
            string ReturnData = "";
            //ReturnData += stIsProcess[index];
            if (stIsProcess.IndexOf(c) != -1)//Có ký tự trong chuỗi
            {
                while (stIsProcess[index] != c)
                {
                    //ReturnData[index] = '0';


                    ReturnData += stIsProcess[index];
                    index++;
                    //b[index] = '0';
                    //stIsProcess.Substring(0, 2) = "q";
                    //StringReader sr = new StringReader(ch);
                    //sr.Read()
                }
                //Loai bo ky tu 
                /*
                Ví dụ
                stIsProcess = "doan1"
                c = a
                i = 0
                return = d
                i = 1
                return = do
                i = 2

                cần cắt doa nên ta gọi stDataFromSerialPort.Remove(0, 3)
                3 ký tự kể từ ký tự đầu tiên
                */
                strDataFromSerialPort = stIsProcess.Remove(0, index + 1);
            }
            return ReturnData;
        }
        /// <summary>
        /// Tìm vị trí chuỗi $GPGGA từ đó tìm gia tốc
        /// khi tìm được chuỗi thì trả về chuỗi mới từ ký tự đầu tiên đến ký tự $
        /// Đồng thời loại bỏ chuỗi từ đầu đến $GPGGA
        /// </summary>
        /// <param name="strIsProcess"></param>
        /// <returns></returns>
        /*
        -0011  0084 -0040 -0003  0012 -0007 -0143 -0016  0976  2687  0175  2041  0849 
        $GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
        $GPVTG,110.33,T,,M,0.18,N,0.33,K,A*34
        */
        public string FindStrGPGGA(string strIsProcess)
        {
            string ReturnData = "";
            if (strIsProcess.IndexOf("$GPGGA") != -1)
            {
                ReturnData = strIsProcess.Substring(0, strIsProcess.IndexOf("$GPGGA") + 1);
                strDataFromSerialPort = strIsProcess.Remove(0, strIsProcess.IndexOf("$GPGGA") + 7);
            }
            return ReturnData;
        }
        /// <summary>
        /// Tìm vị trí của text trong chuỗi string
        /// Lấy phần ở trước chuỗi string
        /// Sau đó cut đi phần trước chuỗi string
        /// </summary>
        /// <param name="strIsProcess"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FindTextInStr(string strIsProcess, string char_cantim)
        {
            /*
                  0006 -0003  1594  0006  0000 -0006  0016  0003  0986 -0161 -0058  0130  1078 
                $GPVTG,,T,,M,0.209,N,0.387,K,D*21
                $GPGGA,063621.90,1045.56915,N,10639.72723,E,2,10,1.93,37.0,M,-2.5,M,,0000*72
            */
            string ReturnData = "";
            ReturnData = strIsProcess.Substring(0, strIsProcess.IndexOf(char_cantim));
            // 0006 -0003  1594  0006  0000 -0006  0016  0003  0986 -0161 -0058  0130  1078
            //GPVTG,,T,,M,0.209,N,0.387,K,D*21
            //GPGGA,063621.90,1045.56915,N,10639.72723,E,2,10,1.93,37.0,M,-2.5,M,,0000*72
            //Save --> .txt
            //SaveTotxt(strIsProcess.Substring(0, strIsProcess.IndexOf(char_cantim) + 2));

            strDataFromSerialPort = strIsProcess.Remove(0, strIsProcess.IndexOf(char_cantim) + 2);
            return ReturnData;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strIsProcess"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FindTextInStr2(string strIsProcess, string text)
        {
            string ReturnData = "";
            ReturnData = strIsProcess.Substring(0, strIsProcess.IndexOf(text));
            strDataFromSerialPort = strIsProcess.Remove(0, strIsProcess.IndexOf(text));
            return ReturnData;
        }


        //**********************************************************************
        //**********************************************************************
        //**********************************************************************
        //**********************************************************************
        /// <summary>
        /// Ngày 03/12/2015 22h38
        /// Tiếp tục test add Image đến map
        /// Ngày 04/12/2015 đã edit dduwwocj ảnh, phóng to thu nhỏ, đặt tại vĩ độ, kinh độ
        /// </summary>
        //Ảnh này là biến toàn cục vì nó có được sử dụng trong chưa trình ngắt
        Image img = new Image();
        public void Image_TestEdit()
        {

            myMap.Children.Remove(img);
            //Image img = sender as Image;
            //BitmapImage bitmapImage = new BitmapImage();
            //img.Width = bitmapImage.DecodePixelWidth = 80; //natural px width of image source
            // don't need to set Height, system maintains aspect ratio, and calculates the other
            // dimension, so long as one dimension measurement is provided
            // bitmapImage.UriSource = new Uri(img.BaseUri, "F:/Entertainment/Photo/picnic/test.jpg");

            //BitmapImage bitmapImage = new BitmapImage();
            //Uri uri = new Uri("ms-appx:///Assets/airplane-icon.png");
            //bitmapImage.UriSource = uri;

            // OR
            //airplane-icon.png có kích thước 129 x 129


            //BitmapImage bitmapImage = new BitmapImage();
            //img.Width = bitmapImage.DecodePixelWidth = 15 * myMap.ZoomLevel;
            //bitmapImage.UriSource = new Uri("ms-appx:///Assets/airplane-icon.png");
            //img.Source = bitmapImage;
            //myMap.ClearValue();

            //Edit size ò image
            img.Height = 15 * myMap.ZoomLevel;
            img.Width = 15 * myMap.ZoomLevel;

            //img.RenderTransform
            img.Opacity = 0.7;


            //img.Transitions.
            img.Source = new BitmapImage(new Uri("ms-appx:///Assets/airplane-icon.png"));
            Image img1 = new Image();
            //img.RenderTransform
            img1.Opacity = 0.7;


            //img.Transitions.
            img1.Source = new BitmapImage(new Uri("ms-appx:///Assets/airplane-icon.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            img.RenderTransform = new RotateTransform()
            {

                //Angle = 3.6 * slider.Value,
                CenterX = 15 * myMap.ZoomLevel / 2,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 15 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img.sca
            img.Stretch = Stretch.Uniform;
            img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            img.Margin = new Windows.UI.Xaml.Thickness(-15 * myMap.ZoomLevel / 2, -15 * myMap.ZoomLevel / 2, 0, 0);
            //tbOutputText.Text = "Latitude: " + myMap.Center.Position.Latitude.ToString() + '\n';
            //tbOutputText.Text += "Longitude: " + myMap.Center.Position.Longitude.ToString() + '\n';
            ////tbOutputText.Text += "Timer Fly: " + Timer_fly.ToString();



            Geopoint PointCenterMap = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude,
                //Altitude = 200.0
            });
            Geopoint PointCenterMap2 = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude + 0.001,
                Longitude = myMap.Center.Position.Longitude + 0.001,
                //Altitude = 200.0
            });
            //myMap.Children.Add(bitmapImage);

            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(img, PointCenterMap);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(img, new Point(0.5, 0.5));
            myMap.Children.Add(img);

            //Vẽ quỹ đạo
            //Draw_Trajectory(Convert.ToDouble(Data.Latitude), Convert.ToDouble(Data.Longtitude), Convert.ToDouble(Data.Altitude));

        }


        private void RotateTransformSample1()

        {

            Rectangle originalRectangle = new Rectangle();

            originalRectangle.Width = 200;

            originalRectangle.Height = 50;

            //originalRectangle = Windows.UI.Xaml.Media.Brush.

            myMap.Children.Add(originalRectangle);



            Rectangle rotatedRectangle = new Rectangle();

            rotatedRectangle.Width = 200;

            rotatedRectangle.Height = 50;

            //rotatedRectangle.Fill = Brushes.Blue;

            rotatedRectangle.Opacity = 0.5;

            //RotateTransform rotateTransform1 = new RotateTransform(45, -50, 50);

            //rotatedRectangle.RenderTransform = rotateTransform1;



            myMap.Children.Add(rotatedRectangle);

        }

        //Ngày 05/12/2015
        //**********************Timer************************
        //**************************************************
        private DispatcherTimer TimerEditImge;
        private void InitTimerEditImage()
        {


            // Start the polling timer.
            TimerEditImge = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(4) };
            TimerEditImge.Tick += TimerEditImge_Tick;
            TimerEditImge.Start();

        }
        /// <summary>
        /// Nguyên nhân của timer là do khi thay đổi zoom level thì ảnh tự nhiên biến mất
        /// nên ta add lại image sau mỗi 4ms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TimerEditImge_Tick(object sender, object e)
        {
            Image_TestEdit();
        }

        private void TwoScreen_Click(object sender, RoutedEventArgs e)
        {
            Background_Sensor(800, -40);
        }

        //******************************************************************
        //*****************************************************************
        //**************Ngày 08/12/2015************************************
        //Vẽ hiển thị của cảm biến
        Image imgAuto = new Image();
        /// <summary>
        /// Chọn background cho các cảm biến
        /// Lấy một hình vẽ bất kỳ làm background
        /// Width: Chiều rộng của hình background
        /// chỉnh lại thành 2 màn hình, khi đó bản đồ bị thu lại
        /// </summary>
        public void Background_Sensor(double Width, double top)
        {

            //Convert to tablet 1366 x 768 --> 1280 x 800;
            if (bDevTablet)
            {
                dConvertToTabletX = 1366 - 1280;
                dConvertToTabletY = 768 - 800;
                tb_ShowTime.Margin = new Windows.UI.Xaml.Thickness(1150, 00, 0, 0);
                btZoomAll.Margin = new Windows.UI.Xaml.Thickness(1150, 60, 0, 0);
                btOneSceen.Margin = new Windows.UI.Xaml.Thickness(1150, 115, 0, 0);
                FindFight.Margin = new Windows.UI.Xaml.Thickness(1150, 170, 0, 0);
                BtMap3D.Margin = new Windows.UI.Xaml.Thickness(1150, 225, 0, 0);
                ConnectDevices.Margin = new Windows.UI.Xaml.Thickness(1150, 280, 0, 0);
                comPortInput.Margin = new Windows.UI.Xaml.Thickness(1150, 370, 0, 0);
                closeDevice.Margin = new Windows.UI.Xaml.Thickness(1150, 425, 0, 0);
                tb_ZoomLevel.Margin = new Windows.UI.Xaml.Thickness(1150, 480, 0, 0);
                tbShowDis.Margin = new Windows.UI.Xaml.Thickness(1150, 550, 0, 0);

                myMap.Height = 762;
                MapBackground.Height = 762;
            }
            //create background left
            FillRect_BackGround(new SolidColorBrush(Colors.LightPink), 0, -300, Width,
            1000 - dConvertToTabletY, 0.7);
            //create background 
            //FillRect_BackGround(new SolidColorBrush(Colors.White), 1236 - dConvertToTabletX, 0, 130,
            //768 - dConvertToTabletY, 0.7);
            //Add textbox Position de co the search duoc
            BackgroundDisplay.Children.Remove(tblock_Position);
            BackgroundDisplay.Children.Remove(tb_Position);
            BackgroundDisplay.Children.Add(tblock_Position);
            BackgroundDisplay.Children.Add(tb_Position);

            BackgroundDisplay.Children.Remove(tb_Lat_Search);
            BackgroundDisplay.Children.Remove(tblock_Latitude);
            BackgroundDisplay.Children.Add(tb_Lat_Search);
            BackgroundDisplay.Children.Add(tblock_Latitude);
            //BackgroundDisplay.Children.Remove(imgAuto);
            //Edit size of image
            //imgAuto.Height = 2080;
            //imgAuto.Width = Width;

            ////imgAuto.RenderTransform
            //imgAuto.Opacity = 1;


            ////imgAuto.Transitions.
            //imgAuto.Source = new BitmapImage(new Uri("ms-appx:///Assets/boat_sailing_in_the_blue_sea_edit3.jpg"));
            ////Xoay ảnh
            ////kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            ////Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            ////khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            ////Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            ////dời ảnh lên trên 1 nửa chiều dài,
            ////dời ảnh sang trái 1 nửa chiều rộng
            //imgAuto.RenderTransform = new RotateTransform()
            //{

            //    //Angle = 3.6 * slider.Value,
            //    //CenterX = 15 * myMap.ZoomLevel / 2,
            //    //CenterX = 62, //The prop name maybe mistyped 
            //    //CenterY = 15 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
            //};
            ////mặc định ảnh có chiều dài và chiều rộng là vô cùng
            ////bitmapImage.PixelHeight
            ////imgAuto.sca
            //imgAuto.Stretch = Stretch.Uniform;
            //imgAuto.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            //imgAuto.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            ////imgAuto.Margin = new Windows.UI.Xaml.Thickness(-100, -imgAuto.Height/3, 00, 00);
            //////tbOutputText.Text = "Height: " + imgAuto.ActualHeight.ToString() + '\n';
            //////tbOutputText.Text += "Width: " + slider.Value.ToString();
            //Geopoint PointCenterMap = new Geopoint(new BasicGeoposition()
            //{
            //    Latitude = myMap.Center.Position.Latitude,
            //    Longitude = myMap.Center.Position.Longitude,
            //    //Altitude = 200.0
            //});
            //Geopoint PointCenterMap2 = new Geopoint(new BasicGeoposition()
            //{
            //    Latitude = myMap.Center.Position.Latitude + 0.001,
            //    Longitude = myMap.Center.Position.Longitude + 0.001,
            //    //Altitude = 200.0
            //});
            //myMap.Children.Add(bitmapImage);

            //Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(imgAuto, PointCenterMap);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(imgAuto, new Point(0.5, 0.5));
            //myMap.Children.Add(imgAuto);
            ////tbOutputText.Background.
            //ckground.a
            //thu bản đồ lại
            myMap.Width = 1363 - dConvertToTabletX - Width;
            myMap.Margin = new Windows.UI.Xaml.Thickness(Width, 0, 00, 00);
            //myMap.Height = 500;
            //Delete các cổng com
            //ConnectDevices.IsEnabled = false;
            //myMap.Children.Remove(ConnectDevices);
            //Background tên là BackgroundDisplay
            //Khi nhấn nút chia 2 màn hình chia là 2 phần
            //phần trái là cảm biến phần bên phải là map
            //chỉnh lại vị trí của ảnh
            //imgAuto.Margin = new Windows.UI.Xaml.Thickness(imgAuto.Width - 2360, top, 00, 00);
            //BackgroundDisplay.Children.Add(imgAuto);
            //Hiện các nút nhấn
            //BackgroundDisplay.Children.Add(mapPolygonAdd);
            //BackgroundDisplay.Children.Remove(ConnectDevices);

            //Remove cac phan không cần thiết
            //BackgroundDisplay.Children.Remove(//tbOutputText);
            //BackgroundDisplay.Children.Remove(sendText);
            //BackgroundDisplay.Children.Remove(sendTextButton);
            //BackgroundDisplay.Children.Remove(status);
            //BackgroundDisplay.Children.Remove(rcvdText);

            //Di chuyển các Listbox và connect disconect đến vị trí thích hợp
            //ConnectDevices.Width = 130;
            //tb_ZoomLevel.Height = 60;
            //comPortInput.Width = 130;
            //closeDevice.Width = 130;
            //tbShowDis.Height = 60;
            //tbShowDis.Width = 130;
            //viết bên xalm
            //ConnectDevices.Margin = new Windows.UI.Xaml.Thickness(1230, 300, 0, 0);
            //comPortInput.Margin = new Windows.UI.Xaml.Thickness(1230, 420, 0, 0);
            //closeDevice.Margin = new Windows.UI.Xaml.Thickness(1230, 470, 0, 0);
            //tbShowDis.Margin = new Windows.UI.Xaml.Thickness(1230, 610, 0, 0);
            //tb_ZoomLevel.Margin = new Windows.UI.Xaml.Thickness(1230, 540, 0, 0);




        }
        //Đã hoàn thành chỉnh 2 màn hình 09/12/2015 0h23p
        //Ngày 09/12/2015 Vẽ các cảm biến
        //*******************************************************************************
        //*******************************************************************************



        /************************************************/
        // Function to draw circle on map:

        private void Test_DrawCircle(BasicGeoposition CenterPosition, double Radius)
        {
            //tạo màu ở trong đường tròn
            Color FillColor = Colors.Blue;
            //tạo màu cho đường tròn
            Color StrokeColor = Colors.Red;
            //độ đục của màu
            FillColor.A = 80;
            StrokeColor.A = 80;
            Windows.UI.Xaml.Controls.Maps.MapPolygon Circle = new Windows.UI.Xaml.Controls.Maps.MapPolygon
            {
                //độ dày của đường tròn
                StrokeThickness = 2,
                FillColor = FillColor,
                StrokeColor = StrokeColor,
                //những điểm màu đường tròn đó đi qua
                Path = new Geopath(CalculateCircle(CenterPosition, Radius))
            };
            myMap.MapElements.Add(Circle);
            //DynamicLabelingInfo minorCityLabelInfo = new DynamicLabelingInfo();
            //minorCityLabelInfo.LabelExpression = "[areaname]";
            //Graphic g = new Graphic();
            //myMap.MapElements.Add(g);
            //myMap.Children.Add(minorCityLabelInfo);
        }


        //Ngày 12/12/2015
        void TestDrawSenSor()
        {
            //Colors.Green.
            BackgroundDisplay.Children.Add(new RingSlice() { StartAngle = -150, EndAngle = -75, Fill = new SolidColorBrush(Colors.Red), Radius = 80, InnerRadius = 50 });
            for (int i = 0; i < 90; i++)
            {
                Brush brush = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, Convert.ToByte(255 * i / 90), 0));
                myMap.Children.Add(
                    new WinRTXamlToolkit.Controls.RingSlice()
                    {
                        StartAngle = i,
                        EndAngle = i + 1,
                        Fill = brush,
                        Radius = 300,
                        InnerRadius = 150,
                        Stroke = brush
                    }
                );
            }
            RingSlice TestRinslice = new RingSlice();
            TestRinslice.StartAngle = 0;
            TestRinslice.EndAngle = 180;
            TestRinslice.Fill = new SolidColorBrush(Colors.Green);
            TestRinslice.Radius = 200;
            TestRinslice.InnerRadius = 100;
            TestRinslice.Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestRinslice);
            RadialGauge TestRadialGauge = new RadialGauge();
            TestRadialGauge.Minimum = 0;
            TestRadialGauge.Maximum = 100;
            TestRadialGauge.Height = 100;
            TestRadialGauge.Width = 100;
            TestRadialGauge.Value = 50.0;
            //TestRadialGauge.TickBrush = Transparent;
            TestRadialGauge.TickBrush = new SolidColorBrush(Colors.Green);
            TestRadialGauge.ScaleTickBrush = new SolidColorBrush(Colors.Green);
            TestRadialGauge.NeedleBrush = new SolidColorBrush(Colors.Blue);
            TestRadialGauge.TrailBrush = new SolidColorBrush(Colors.Blue);
            TestRadialGauge.ValueBrush = new SolidColorBrush(Colors.Blue);
            TestRadialGauge.Unit = "meter";
            TestRadialGauge.UnitBrush = new SolidColorBrush(Colors.Blue);
            TestRadialGauge.Margin = new Windows.UI.Xaml.Thickness(-1000, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestRadialGauge);


        }
        /// <summary>
        /// Vẽ hình chữ nhật
        /// </summary>
        void TestDrawRectangle()
        {
            Rectangle TestRetangle = new Rectangle();
            TestRetangle.Fill = new SolidColorBrush(Colors.Green);
            TestRetangle.Height = 500;
            TestRetangle.Width = 200;
            //Xac định tọa độ
            TestRetangle.Margin = new Windows.UI.Xaml.Thickness(-1358 + TestRetangle.Width, -798 + dConvertToTabletY + TestRetangle.Height, 0, 0);
            BackgroundDisplay.Children.Add(TestRetangle);

        }
        /// <summary>
        /// Vẽ đường thẳng
        /// </summary>
        void TestDrawLine()
        {
            Line TestLine = new Line();
            TestLine.Fill = new SolidColorBrush(Colors.Green);
            TestLine.Stroke = new SolidColorBrush(Colors.Red);
            //TestLine.
            //TestLine.Height = 10;
            //TestLine.Width = 10;
            TestLine.X1 = 50;
            TestLine.Y1 = 50;
            TestLine.X2 = 350;
            TestLine.Y2 = 50;
            TestLine.StrokeThickness = 10;
            //Xac định tọa độ
            //TestLine.Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestLine);

        }

        /// <summary>
        /// Vẽ tam giác
        /// </summary>
        void TestDrawTriAngle()
        {


            Polygon myPolygon = new Polygon();
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(0, 0));
            myPointCollection.Add(new Point(0, 0.025));
            myPointCollection.Add(new Point(0.015, 0.005));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));

            //BackgroundDisplay.Children.Remove(myPolygon);
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = new SolidColorBrush(Colors.Green);
            myPolygon.Width = 100;
            myPolygon.Height = 100;
            myPolygon.Stretch = Stretch.Fill;
            myPolygon.Stroke = new SolidColorBrush(Colors.Black);
            myPolygon.StrokeThickness = 1;
            //Xac định tọa độ
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(- 1358, -498, 0, 0);
            BackgroundDisplay.Children.Add(myPolygon);

        }

        //*************************************************************
        //************************************************************
        //Ve hinh


        /// <summary>
        /// A Modern UI Radial Gauge.
        /// </summary>
        [TemplatePart(Name = NeedlePartName, Type = typeof(Windows.UI.Xaml.Shapes.Path))]
        [TemplatePart(Name = ScalePartName, Type = typeof(Windows.UI.Xaml.Shapes.Path))]
        [TemplatePart(Name = TrailPartName, Type = typeof(Windows.UI.Xaml.Shapes.Path))]
        [TemplatePart(Name = ValueTextPartName, Type = typeof(TextBlock))]
        public class RadialGauge : Control
        {
            #region Constants

            private const string NeedlePartName = "PART_Needle";

            private const string ScalePartName = "PART_Scale";

            private const string TrailPartName = "PART_Trail";

            private const string ValueTextPartName = "PART_ValueText";

            private const double Degrees2Radians = Math.PI / 180;

            #endregion Constants

            #region Dependency Property Registrations

            public static readonly DependencyProperty MinimumProperty =
                DependencyProperty.Register("Minimum", typeof(double), typeof(RadialGauge), new PropertyMetadata(0.0));

            public static readonly DependencyProperty MaximumProperty =
                DependencyProperty.Register("Maximum", typeof(double), typeof(RadialGauge), new PropertyMetadata(100.0));

            public static readonly DependencyProperty ScaleWidthProperty =
                DependencyProperty.Register("ScaleWidth", typeof(double), typeof(RadialGauge), new PropertyMetadata(26.0));

            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(double), typeof(RadialGauge), new PropertyMetadata(0.0, OnValueChanged));

            public static readonly DependencyProperty UnitProperty =
                DependencyProperty.Register("Unit", typeof(string), typeof(RadialGauge), new PropertyMetadata(string.Empty));

            public static readonly DependencyProperty NeedleBrushProperty =
                DependencyProperty.Register("NeedleBrush", typeof(Brush), typeof(RadialGauge), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

            public static readonly DependencyProperty ScaleBrushProperty =
                DependencyProperty.Register("ScaleBrush", typeof(Brush), typeof(RadialGauge), new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

            public static readonly DependencyProperty TickBrushProperty =
                DependencyProperty.Register("TickBrush", typeof(Brush), typeof(RadialGauge), new PropertyMetadata(new SolidColorBrush(Colors.White)));

            public static readonly DependencyProperty TrailBrushProperty =
                DependencyProperty.Register("TrailBrush", typeof(Brush), typeof(RadialGauge), new PropertyMetadata(new SolidColorBrush(Colors.Orange)));

            public static readonly DependencyProperty ValueBrushProperty =
                DependencyProperty.Register("ValueBrush", typeof(Brush), typeof(RadialGauge), new PropertyMetadata(new SolidColorBrush(Colors.White)));

            public static readonly DependencyProperty ScaleTickBrushProperty =
                DependencyProperty.Register("ScaleTickBrush", typeof(Brush), typeof(RadialGauge), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

            public static readonly DependencyProperty UnitBrushProperty =
                DependencyProperty.Register("UnitBrush", typeof(Brush), typeof(RadialGauge), new PropertyMetadata(new SolidColorBrush(Colors.White)));

            public static readonly DependencyProperty ValueStringFormatProperty =
                DependencyProperty.Register("ValueStringFormat", typeof(string), typeof(RadialGauge), new PropertyMetadata("N0"));

            public static readonly DependencyProperty TickSpacingProperty =
            DependencyProperty.Register("TickSpacing", typeof(int), typeof(RadialGauge), new PropertyMetadata(10));

            protected static readonly DependencyProperty ValueAngleProperty =
                DependencyProperty.Register("ValueAngle", typeof(double), typeof(RadialGauge), new PropertyMetadata(null));

            protected static readonly DependencyProperty TicksProperty =
                DependencyProperty.Register("Ticks", typeof(IEnumerable<double>), typeof(RadialGauge), new PropertyMetadata(null));

            #endregion Dependency Property Registrations

            #region Constructors

            public RadialGauge()
            {
                this.DefaultStyleKey = typeof(RadialGauge);
                this.Ticks = this.getTicks();
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Gets or sets the minimum on the scale.
            /// </summary>
            public double Minimum
            {
                get { return (double)GetValue(MinimumProperty); }
                set { SetValue(MinimumProperty, value); }
            }

            /// <summary>
            /// Gets or sets the maximum on the scale.
            /// </summary>
            public double Maximum
            {
                get { return (double)GetValue(MaximumProperty); }
                set { SetValue(MaximumProperty, value); }
            }

            /// <summary>
            /// Gets or sets the width of the scale.
            /// </summary>
            public double ScaleWidth
            {
                get { return (double)GetValue(ScaleWidthProperty); }
                set { SetValue(ScaleWidthProperty, value); }
            }

            /// <summary>
            /// Gets or sets the current value.
            /// </summary>
            public double Value
            {
                get { return (double)GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }

            /// <summary>
            /// Gets or sets the unit measure.
            /// </summary>
            public string Unit
            {
                get { return (string)GetValue(UnitProperty); }
                set { SetValue(UnitProperty, value); }
            }

            /// <summary>
            /// Gets or sets the needle brush.
            /// </summary>
            public Brush NeedleBrush
            {
                get { return (Brush)GetValue(NeedleBrushProperty); }
                set { SetValue(NeedleBrushProperty, value); }
            }

            /// <summary>
            /// Gets or sets the trail brush.
            /// </summary>
            public Brush TrailBrush
            {
                get { return (Brush)GetValue(TrailBrushProperty); }
                set { SetValue(TrailBrushProperty, value); }
            }

            /// <summary>
            /// Gets or sets the scale brush.
            /// </summary>
            public Brush ScaleBrush
            {
                get { return (Brush)GetValue(ScaleBrushProperty); }
                set { SetValue(ScaleBrushProperty, value); }
            }

            /// <summary>
            /// Gets or sets the scale tick brush.
            /// </summary>
            public Brush ScaleTickBrush
            {
                get { return (Brush)GetValue(ScaleTickBrushProperty); }
                set { SetValue(ScaleTickBrushProperty, value); }
            }

            /// <summary>
            /// Gets or sets the outer tick brush.
            /// </summary>
            public Brush TickBrush
            {
                get { return (Brush)GetValue(TickBrushProperty); }
                set { SetValue(TickBrushProperty, value); }
            }

            /// <summary>
            /// Gets or sets the value brush.
            /// </summary>
            public Brush ValueBrush
            {
                get { return (Brush)GetValue(ValueBrushProperty); }
                set { SetValue(ValueBrushProperty, value); }
            }

            /// <summary>
            /// Gets or sets the unit brush.
            /// </summary>
            public Brush UnitBrush
            {
                get { return (Brush)GetValue(UnitBrushProperty); }
                set { SetValue(UnitBrushProperty, value); }
            }

            /// <summary>
            /// Gets or sets the value string format.
            /// </summary>
            public string ValueStringFormat
            {
                get { return (string)GetValue(ValueStringFormatProperty); }
                set { SetValue(ValueStringFormatProperty, value); }
            }

            /// <summary>
            /// Gets or sets the tick spacing, in units.
            /// </summary>
            public int TickSpacing
            {
                get { return (int)GetValue(TickSpacingProperty); }
                set { SetValue(TickSpacingProperty, value); }
            }

            protected double ValueAngle
            {
                get { return (double)GetValue(ValueAngleProperty); }
                set { SetValue(ValueAngleProperty, value); }
            }

            public IEnumerable<double> Ticks
            {
                get { return (IEnumerable<double>)GetValue(TicksProperty); }
                protected set { SetValue(TicksProperty, value); }
            }

            #endregion Properties

            protected override void OnApplyTemplate()
            {
                // Draw Scale
                var scale = this.GetTemplateChild(ScalePartName) as Windows.UI.Xaml.Shapes.Path;
                if (scale != null)
                {
                    var pg = new PathGeometry();
                    var pf = new PathFigure();
                    pf.IsClosed = false;
                    var middleOfScale = 77 - this.ScaleWidth / 2;
                    pf.StartPoint = this.ScalePoint(-150, middleOfScale);
                    var seg = new ArcSegment();
                    seg.SweepDirection = SweepDirection.Clockwise;
                    seg.IsLargeArc = true;
                    seg.Size = new Size(middleOfScale, middleOfScale);
                    seg.Point = this.ScalePoint(150, middleOfScale);
                    pf.Segments.Add(seg);
                    pg.Figures.Add(pf);
                    scale.Data = pg;
                }

                OnValueChanged(this);
                base.OnApplyTemplate();
            }

            private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                OnValueChanged(d);
            }

            private static void OnValueChanged(DependencyObject d)
            {
                RadialGauge c = (RadialGauge)d;
                if (!double.IsNaN(c.Value))
                {
                    var middleOfScale = 77 - c.ScaleWidth / 2;
                    var needle = c.GetTemplateChild(NeedlePartName) as Windows.UI.Xaml.Shapes.Path;
                    var valueText = c.GetTemplateChild(ValueTextPartName) as TextBlock;
                    c.ValueAngle = c.ValueToAngle(c.Value);

                    // Needle
                    if (needle != null)
                    {
                        needle.RenderTransform = new RotateTransform() { Angle = c.ValueAngle };
                    }

                    // Trail
                    var trail = c.GetTemplateChild(TrailPartName) as Windows.UI.Xaml.Shapes.Path;
                    if (trail != null)
                    {
                        if (c.ValueAngle > -146)
                        {
                            trail.Visibility = Visibility.Visible;
                            var pg = new PathGeometry();
                            var pf = new PathFigure();
                            pf.IsClosed = false;
                            pf.StartPoint = c.ScalePoint(-150, middleOfScale);
                            var seg = new ArcSegment();
                            seg.SweepDirection = SweepDirection.Clockwise;
                            // We start from -150, so +30 becomes a large arc.
                            seg.IsLargeArc = c.ValueAngle > 30;
                            seg.Size = new Size(middleOfScale, middleOfScale);
                            seg.Point = c.ScalePoint(c.ValueAngle, middleOfScale);
                            pf.Segments.Add(seg);
                            pg.Figures.Add(pf);
                            trail.Data = pg;
                        }
                        else
                        {
                            trail.Visibility = Visibility.Collapsed;
                        }
                    }

                    // Value Text
                    if (valueText != null)
                    {
                        valueText.Text = c.Value.ToString(c.ValueStringFormat);
                    }
                }
            }

            private Point ScalePoint(double angle, double middleOfScale)
            {
                return new Point(100 + Math.Sin(Degrees2Radians * angle) * middleOfScale, 100 - Math.Cos(Degrees2Radians * angle) * middleOfScale);
            }

            private double ValueToAngle(double value)
            {
                double minAngle = -150;
                double maxAngle = 150;

                // Off-scale to the left
                if (value < this.Minimum)
                {
                    return minAngle - 7.5;
                }

                // Off-scale to the right
                if (value > this.Maximum)
                {
                    return maxAngle + 7.5;
                }

                double angularRange = maxAngle - minAngle;

                return (value - this.Minimum) / (this.Maximum - this.Minimum) * angularRange + minAngle;
            }

            private IEnumerable<double> getTicks()
            {
                double tickSpacing = TickSpacing;
                // double tickSpacing = (this.Maximum - this.Minimum) / 10;
                for (double tick = this.Minimum; tick <= this.Maximum; tick += tickSpacing)
                {
                    yield return ValueToAngle(tick);
                }
            }

        }
        //*******************************************************
        //Ngày 13/12/2015
        //int x = 210, y = 200, width = 210, height = 210, startAngle = 210, sweepAngle = 120;


        /// <summary>
        /// Hàm vẽ đường tròn, cung tròn có góc bắt đầu là startAngle, góc quét là sweepAngle
        /// đường tròn nội tiếp hình chữ nhật có điểm bắt đầu là x, y và rộng width cao height
        /// Chú ý góc quét phải nhỏ hơn 360
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        void DrawArc(double x, double y, double width, double height, int startAngle, int sweepAngle)

        {


            RingSlice TestRinslice = new RingSlice();
            //BackgroundDisplay.Children.Remove(TestRinslice);
            TestRinslice.StartAngle = (double)startAngle + 90;
            TestRinslice.EndAngle = startAngle + 90 + sweepAngle;
            TestRinslice.Fill = new SolidColorBrush(Colors.Green);
            TestRinslice.Radius = height / 2;
            TestRinslice.InnerRadius = height / 2 - 3;
            //Thickness sẽ dời tâm đường tròn

            TestRinslice.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + dConvertToTabletX + (TestRinslice.Radius + x) * 2, -800 + dConvertToTabletY + (TestRinslice.Radius + y) * 2, 0, 0);
            BackgroundDisplay.Children.Add(TestRinslice);

        }

        //************************************************************************
        //*******************************************************
        //Ngày 19/12/2015
        //int x = 210, y = 200, width = 210, height = 210, startAngle = 210, sweepAngle = 120;


        /// <summary>
        /// Hàm vẽ đường tròn, cung tròn có góc bắt đầu là startAngle, góc quét là sweepAngle
        /// đường tròn nội tiếp hình chữ nhật có điểm bắt đầu là x, y và rộng width cao height
        /// Chú ý góc quét phải nhỏ hơn 360
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        void Compass_DrawArc(SolidColorBrush brush, double x, double y, double width, double height, int startAngle, int sweepAngle)

        {


            RingSlice TestRinslice = new RingSlice();
            //BackgroundDisplay.Children.Remove(TestRinslice);
            TestRinslice.StartAngle = (double)startAngle + 90;
            TestRinslice.EndAngle = startAngle + 90 + sweepAngle;
            TestRinslice.Fill = brush;
            TestRinslice.Radius = height / 2;
            TestRinslice.InnerRadius = height / 2 - 3;
            //Thickness sẽ dời tâm đường tròn

            TestRinslice.Margin = new Windows.UI.Xaml.Thickness(
                -2357 + (TestRinslice.Radius + x) * 2, -799 + (TestRinslice.Radius + y) * 2, 0, 0);
            BackgroundDisplay.Children.Add(TestRinslice);

        }
        void RollAngle_Setup(double Roll, double dBalance_mid_X, double dBalance_mid_Y, double dBalance_R)
        {

            /************************************************************/
            //double dBalance_mid_X = 400, dBalance_mid_Y = 200, dBalance_R = 120;

            //ngay 28/09/2015
            //dormGraphic.DrawArc(whitePen, 200, 50, 200, 200, 210, 120);
            //Ve duong tron ben trong I(dmidXComPass, dComPass_mid_Y) ban kinh dRIntoComPass

            //dormGraphic.DrawArc(whitePen, (dComPass_mid_X - dComPass_R_Into), dComPass_mid_Y - dComPass_R_Into,
            // 2 * dComPass_R_Into, 2 * dComPass_R_Into, 0, 360);
            //Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            double dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into;
            /*
            dArc_X_15 = dComPass_R * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            dArc_Y_15 = - dComPass_R * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            dArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            dArc_Y_15_Into = - dComPass_R_Into * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            dormGraphic.DrawLine(greenPen, dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into);
             */
            //dormGraphic.DrawPie(redPen, 100, 100, 100, 100, 0, 90);
            // Draw curve to screen.
            //dormGraphic.DrawCurve(greenPen, curvePoints);

            //rmGraphic.DrawCurve(new Pen(Color.Red), 0, 0, 100);
            //rmGraphic.dillRectangle(Brushes.Green, 0, 0, 300, 300);
            //***********************************************************
            //***********************************************************
            /*
             * *Ngày 29/09/2015
             */
            //dormGraphic.DrawArc(whitePen, 200, 50, 200, 200, 210, 120);

            //Ve duong tron ben ngoai I(dBalance_mid_X, dBalance_mid_Y) ban kinh dBalance_R
            //dormGraphic.DrawArc(whitePen, (dBalance_mid_X - dBalance_R), dBalance_mid_Y - dBalance_R,
            // 2 * dBalance_R, 2 * dBalance_R, 210, 120);
            //Ve duong tron ben trong I(dmidXBalance, dBalance_mid_Y) ban kinh dRIntoBalance
            //Ghi chữ Attitude (thái độ, dáng điệu) And Director (đang đi lên hay đi xuống) của máy bay
            DrawString("Attitude And Director (degrees)", 20, new SolidColorBrush(Colors.Yellow),
                dBalance_mid_X - dBalance_R, dBalance_mid_Y - dBalance_R - 25, 1);
            double dBalance_R_Into;
            dBalance_R_Into = dBalance_R - 15;
            DrawArc((dBalance_mid_X - dBalance_R_Into), dBalance_mid_Y - dBalance_R_Into,
            2 * dBalance_R_Into, 2 * dBalance_R_Into, 210, 120);

            //Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            // double dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into;
            //Ve cac duong net do chia cho cung tron o tren
            //Cung tron 120 do
            //Ve tai 30, 60, 90, 120, 150 voi duong dai 15
            //Lay duong trong trong lam moc
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.Green);
            dBalance_R_Into = dBalance_R - 15;
            for (int index = 30; index <= 150; index += 30)
            {
                dArc_X_15 = dBalance_R * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15 = -dBalance_R * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                dArc_X_15_Into = dBalance_R_Into * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15_Into = -dBalance_R_Into * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                DrawLine(BlushOfLine1, 2, dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into);
            }
            //Ve tai 45, 135 voi duong dai 10
            dBalance_R = dBalance_R_Into + 10;
            for (int index = 45; index <= 150; index += 90)
            {
                dArc_X_15 = dBalance_R * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15 = -dBalance_R * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                dArc_X_15_Into = dBalance_R_Into * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15_Into = -dBalance_R_Into * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                DrawLine(BlushOfLine1, 2, dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into);
            }
            //Ve tai 70, 80, 100, 110 voi duong dai 10
            dBalance_R = dBalance_R_Into + 10;
            for (int index = 70; index <= 110; index += 10)
            {
                dArc_X_15 = dBalance_R * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15 = -dBalance_R * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                dArc_X_15_Into = dBalance_R_Into * (double)Math.Cos(Math.PI * index / 180) + dBalance_mid_X;
                dArc_Y_15_Into = -dBalance_R_Into * (double)Math.Sin(Math.PI * index / 180) + dBalance_mid_Y;
                DrawLine(BlushOfLine1, 2, dArc_X_15, dArc_Y_15, dArc_X_15_Into, dArc_Y_15_Into);
            }
            /************************************************************/
            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = dBalance_mid_X;
            y1 = (dBalance_mid_Y - dBalance_R_Into);
            x2 = x1 - 10;
            y2 = y1 - 10;
            x3 = x1 + 10;
            y3 = y1 - 10;
            Polygon(x1, y1, x2, y2, x3, y3);
            //Vẽ mũi tên chạy chạy
            RollAngle_PolygonAutoRemove_setup();
            RollAngle_Draw_TriAngle(Roll, dBalance_mid_X, dBalance_mid_Y, dBalance_R);

        }
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void RollAngle_PolygonAutoRemove_setup()
        {
            SolidColorBrush BlushOfTriAngle = new SolidColorBrush(Colors.Blue);
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));
            //Polygon myRollAngle_PolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(myRollAngle_PolygonAutoRemove);
            //myRollAngle_PolygonAutoRemove.Points = myPointCollection;
            myRollAngle_PolygonAutoRemove.Fill = BlushOfTriAngle;
            myRollAngle_PolygonAutoRemove.Stretch = Stretch.Fill;
            myRollAngle_PolygonAutoRemove.Stroke = BlushOfTriAngle;
            myRollAngle_PolygonAutoRemove.Opacity = 0.8;
            myRollAngle_PolygonAutoRemove.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + myRollAngle_PolygonAutoRemove.Width - (200 - 2 * xmin), -600 + myRollAngle_PolygonAutoRemove.Height, 0, 0);
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + myRollAngle_PolygonAutoRemove.Height + 4 * slider.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -800  +  dConvertToTabletY + ymax + ymin, 0, 0);
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            //BackgroundDisplay.Children.Add(myRollAngle_PolygonAutoRemove);


        }
        //************************************************************************

        /// <summary>
        /// Vẽ đường thẳng từ (x1, y1) đến (x2, y2)
        /// Bút vẽ là ColorOfLine
        /// độ rông là SizeOfLine
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        void DrawLine(SolidColorBrush ColorOfLine, double SizeOfLine, double x1, double y1, double x2, double y2)
        {
            Line TestLine = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line TestLine = new Line();
            //BackgroundDisplay.Children.Remove(TestLine);
            //TestLine.Fill = new SolidColorBrush(Colors.Green);
            TestLine.Stroke = ColorOfLine;
            //TestLine.
            //TestLine.Height = 10;
            //TestLine.Width = 10;
            TestLine.X1 = x1;
            TestLine.Y1 = y1;
            TestLine.X2 = x2;
            TestLine.Y2 = y2;
            TestLine.StrokeThickness = SizeOfLine;
            //Xac định tọa độ
            //TestLine.Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestLine);

        }
        //Vẽ Line Auto Remove cho Compass
        Line[] LineCompassAutoRemove = new Line[125];
        /// <summary>
        /// Vẽ đường thẳng từ (x1, y1) đến (x2, y2)
        /// Bút vẽ là ColorOfLine
        /// độ rông là SizeOfLine
        /// index: chỉ số của đường
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        void Compass_LineAutoRemove(int index, SolidColorBrush ColorOfLine, double SizeOfLine)
        {
            //BackgroundDisplay.Children.Remove(LineCompassAutoRemove[index]);
            LineCompassAutoRemove[index] = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line LineCompassAutoRemove[index] = new Line();
            //BackgroundDisplay.Children.Remove(LineCompassAutoRemove[index]);
            //LineCompassAutoRemove[index].Fill = new SolidColorBrush(Colors.Green);
            LineCompassAutoRemove[index].Stroke = ColorOfLine;
            //LineCompassAutoRemove[index].
            //LineCompassAutoRemove[index].Height = 10;
            //LineCompassAutoRemove[index].Width = 10;
            LineCompassAutoRemove[index].StrokeThickness = SizeOfLine;
            //Xac định tọa độ
            //LineCompassAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            //BackgroundDisplay.Children.Add(LineCompassAutoRemove[index]);

        }
        void Compass_LineAutoRemove_run(int index, double x1, double y1, double x2, double y2)
        {
            BackgroundDisplay.Children.Remove(LineCompassAutoRemove[index]);
            //LineCompassAutoRemove[index] = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line LineCompassAutoRemove[index] = new Line();
            //BackgroundDisplay.Children.Remove(LineCompassAutoRemove[index]);
            //LineCompassAutoRemove[index].Fill = new SolidColorBrush(Colors.Green);
            //LineCompassAutoRemove[index].Stroke = ColorOfLine;
            //LineCompassAutoRemove[index].
            //LineCompassAutoRemove[index].Height = 10;
            //LineCompassAutoRemove[index].Width = 10;
            LineCompassAutoRemove[index].X1 = x1;
            LineCompassAutoRemove[index].Y1 = y1;
            LineCompassAutoRemove[index].X2 = x2;
            LineCompassAutoRemove[index].Y2 = y2;
            //LineCompassAutoRemove[index].StrokeThickness = SizeOfLine;
            //Xac định tọa độ
            //LineCompassAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            BackgroundDisplay.Children.Add(LineCompassAutoRemove[index]);

        }

        //Ngày 12/12/2015
        RingSlice TestRinslice = new RingSlice();
        void TestDrawSenSor1()
        {

            BackgroundDisplay.Children.Remove(TestRinslice);
            TestRinslice.StartAngle = 0;
            TestRinslice.EndAngle = 359.999;
            TestRinslice.Fill = new SolidColorBrush(Colors.Green);
            TestRinslice.Radius = 400;
            TestRinslice.InnerRadius = 350;
            TestRinslice.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + TestRinslice.Radius * 2, -798 + dConvertToTabletY + TestRinslice.Radius * 2, 0, 0);
            BackgroundDisplay.Children.Add(TestRinslice);
        }
        //************************************************************
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Polygon(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw
            Polygon myPolygon = new Polygon();
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(x1 - xmin, y1 - ymin));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));

            //BackgroundDisplay.Children.Remove(myPolygon);
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = new SolidColorBrush(Colors.Green);
            myPolygon.Width = xmax - xmin;
            myPolygon.Height = ymax - ymin;
            myPolygon.Stretch = Stretch.Fill;
            myPolygon.Stroke = new SolidColorBrush(Colors.Black);
            myPolygon.StrokeThickness = 1;
            //Xac định tọa độ -2060, -491
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2338 + xmin * 2, -786 + 2 * ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygon.Width, -600 + myPolygon.Height, 0, 0);
            myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2250 , -486, 0, 0);
            BackgroundDisplay.Children.Add(myPolygon);


        }
        //************************************************************************
        //************************************************************
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void PolygonHaveBrush(SolidColorBrush brush, double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw
            Polygon myPolygon = new Polygon();
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(x1 - xmin, y1 - ymin));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));

            //BackgroundDisplay.Children.Remove(myPolygon);
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = brush;
            myPolygon.Width = xmax - xmin;
            myPolygon.Height = ymax - ymin;
            myPolygon.Stretch = Stretch.Fill;
            //màu viền
            myPolygon.Stroke = brush;
            myPolygon.StrokeThickness = 1;
            //Xac định tọa độ -2060, -491
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2338 + xmin * 2, -786 + 2 * ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygon.Width, -600 + myPolygon.Height, 0, 0);
            myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygon.Margin = new Windows.UI.Xaml.Thickness(-2250 , -486, 0, 0);
            BackgroundDisplay.Children.Add(myPolygon);


        }
        //************************************************************************
        //************************************************************
        //Ngày 14/1/2/2015 22h39 đã hoàn thành việc vẽ tam giác đúng vị trí
        //Hoàn thahf vẽ display cảm biến gia tốc
        Polygon myPolygonAutoRemove = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void PolygonAutoRemove(SolidColorBrush BlushOfTriAngle, double x1, double y1, double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));
            //Polygon myPolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(myPolygonAutoRemove);
            myPolygonAutoRemove.Points = myPointCollection;
            myPolygonAutoRemove.Fill = BlushOfTriAngle;
            myPolygonAutoRemove.Width = (xmax - xmin);
            myPolygonAutoRemove.Height = (ymax - ymin);
            myPolygonAutoRemove.Stretch = Stretch.Fill;
            myPolygonAutoRemove.Stroke = BlushOfTriAngle;
            myPolygonAutoRemove.Opacity = Opacity;
            myPolygonAutoRemove.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygonAutoRemove.Width - (200 - 2 * xmin), -600 + myPolygonAutoRemove.Height, 0, 0);
            //myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + myPolygonAutoRemove.Height + 4 * slider.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(myPolygonAutoRemove);


        }

        //************************************************************
        //Ngày 14/1/2/2015 22h39 đã hoàn thành việc vẽ tam giác đúng vị trí
        //Hoàn thahf vẽ display cảm biến gia tốc
        Polygon myPolygonAutoRemove_Alt = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Altitude_PolygonAutoRemove(SolidColorBrush BlushOfTriAngle, double x1, double y1, double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));
            //Polygon myPolygonAutoRemove_Alt = new Polygon();
            BackgroundDisplay.Children.Remove(myPolygonAutoRemove_Alt);
            myPolygonAutoRemove_Alt.Points = myPointCollection;
            myPolygonAutoRemove_Alt.Fill = BlushOfTriAngle;
            myPolygonAutoRemove_Alt.Width = (xmax - xmin);
            myPolygonAutoRemove_Alt.Height = (ymax - ymin);
            myPolygonAutoRemove_Alt.Stretch = Stretch.Fill;
            myPolygonAutoRemove_Alt.Stroke = BlushOfTriAngle;
            myPolygonAutoRemove_Alt.Opacity = Opacity;
            myPolygonAutoRemove_Alt.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2158 + myPolygonAutoRemove_Alt.Width - (200 - 2 * xmin), -600 + myPolygonAutoRemove_Alt.Height, 0, 0);
            //myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + myPolygonAutoRemove_Alt.Height + 4 * slider.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myPolygonAutoRemove_Alt.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(myPolygonAutoRemove_Alt);

        }
        //*********************************************************************************************

        //*********************************************************************************************
        //Ngày 14/12/2015
        /// <summary>
        /// Vẽ mũi tên chạy qua chạy lại của bộ cảm biến gia tốc
        /// Roll là góc quay
        /// </summary>
        void RollAngle_Draw_TriAngle(double Roll, double dBalance_mid_X, double dBalance_mid_Y, double dBalance_R)
        {
            Roll = 90 - Roll;
            //*********************************************************
            //Ngay 30/09/2015
            //bien xac dinh tam va ban kinh cung tron
            //double dBalance_mid_X = 400, dBalance_mid_Y = 200, dBalance_R = 120;
            //bien xac dinh 3 diem cua tam giac
            double dTriAngle_P1_X, dTriAngle_P1_Y, dTriAngle_P2_X, dTriAngle_P2_Y, dTriAngle_P3_X, dTriAngle_P3_Y;
            double dBalance_R_Into, temp1, temp2, temp3;//cac biến tạm để tối ưu
            //Lay duong trong trong lam moc
            //Diem point1 cua tam giac
            dBalance_R_Into = dBalance_R - 16;
            temp1 = Math.PI * Roll / 180;
            dTriAngle_P1_X = dBalance_R_Into * (double)Math.Cos(temp1) + dBalance_mid_X;
            dTriAngle_P1_Y = -dBalance_R_Into * (double)Math.Sin(temp1) + dBalance_mid_Y;
            //Diem point2 cua tam giac
            dBalance_R_Into = dBalance_R - 30;
            temp2 = Math.PI * (Roll + 10) / 180;
            dTriAngle_P2_X = dBalance_R_Into * (double)Math.Cos(temp2) + dBalance_mid_X;
            dTriAngle_P2_Y = -dBalance_R_Into * (double)Math.Sin(temp2) + dBalance_mid_Y;
            //Diem point3 cua tam giac
            dBalance_R_Into = dBalance_R - 30;
            temp3 = Math.PI * (Roll - 10) / 180;
            dTriAngle_P3_X = dBalance_R_Into * (double)Math.Cos(temp3) + dBalance_mid_X;
            dTriAngle_P3_Y = -dBalance_R_Into * (double)Math.Sin(temp3) + dBalance_mid_Y;
            //Vẽ tam giác qua 3 điểm
            RollAngle_PolygonAutoRemove(dTriAngle_P1_X, dTriAngle_P1_Y, dTriAngle_P2_X, dTriAngle_P2_Y, dTriAngle_P3_X, dTriAngle_P3_Y);
            //Point[] points = { new Point((int)dTriAngle_P1_X, (int)dTriAngle_P1_Y), new Point
            //        ((int)dTriAngle_P2_X, (int)dTriAngle_P2_Y), new Point((int)dTriAngle_P3_X, (int)dTriAngle_P3_Y) };
            //G_TriAngle.dillPolygon(Blush_TriAngle, points);
            //Ve tai 45, 135


        }
        //*********************************************************************************************
        //Hoàn thanh vẽ display cảm biến gia tốc
        Polygon myRollAngle_PolygonAutoRemove = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void RollAngle_PolygonAutoRemove(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));
            //Polygon myRollAngle_PolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(myRollAngle_PolygonAutoRemove);
            myRollAngle_PolygonAutoRemove.Points = myPointCollection;
            myRollAngle_PolygonAutoRemove.Width = (xmax - xmin);
            myRollAngle_PolygonAutoRemove.Height = (ymax - ymin);
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + myRollAngle_PolygonAutoRemove.Width - (200 - 2 * xmin), -600 + myRollAngle_PolygonAutoRemove.Height, 0, 0);
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + myRollAngle_PolygonAutoRemove.Height + 4 * slider.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //myRollAngle_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(myRollAngle_PolygonAutoRemove);


        }
        //*********************************************************************************************
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRectangle(SolidColorBrush Blush, double StartX, double StartY, double width, double height)
        {
            Rectangle TestRetangle = new Rectangle();
            TestRetangle.Fill = Blush;
            TestRetangle.Height = height;
            TestRetangle.Width = width;
            TestRetangle.Opacity = 0.7;
            //Xac định tọa độ
            TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + dConvertToTabletX + TestRetangle.Width + StartX * 2, -798 + dConvertToTabletY + TestRetangle.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestRetangle);
        }
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRectangleHaveOpacity(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            Rectangle TestRetangle = new Rectangle();
            TestRetangle.Fill = Blush;
            TestRetangle.Height = height;
            TestRetangle.Width = width;
            TestRetangle.Opacity = Opacity;
            //Xac định tọa độ
            TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + TestRetangle.Width + StartX * 2, -798 + dConvertToTabletY + TestRetangle.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestRetangle);
        }
        //********************************************************************************
        //Vẽ những hình Chữ nhật auto remove
        Rectangle RetangleAutoRemove1 = new Rectangle();
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRect_AutoRemove1(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(RetangleAutoRemove1);
            RetangleAutoRemove1.Fill = Blush;
            RetangleAutoRemove1.Height = height;
            RetangleAutoRemove1.Width = width;
            RetangleAutoRemove1.Opacity = Opacity;
            //Xac định tọa độ
            RetangleAutoRemove1.Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + RetangleAutoRemove1.Width + StartX * 2, -798 + dConvertToTabletY + RetangleAutoRemove1.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(RetangleAutoRemove1);
        }
        //********************************************************************************
        //********************************************************************************
        //Vẽ những hình Chữ nhật auto remove
        Rectangle RetangleAutoRemove2 = new Rectangle();
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRect_AutoRemove2(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(RetangleAutoRemove2);
            RetangleAutoRemove2.Fill = Blush;
            RetangleAutoRemove2.Height = height;
            RetangleAutoRemove2.Width = width;
            RetangleAutoRemove2.Opacity = Opacity;
            //Xac định tọa độ
            RetangleAutoRemove2.Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + RetangleAutoRemove2.Width + StartX * 2, -798 + dConvertToTabletY + RetangleAutoRemove2.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(RetangleAutoRemove2);
        }
        //********************************************************************************
        //********************************************************************************
        //Vẽ những hình Chữ nhật auto remove
        Rectangle RetangleAutoRemove3 = new Rectangle();
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRect_AutoRemove3(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(RetangleAutoRemove3);
            RetangleAutoRemove3.Fill = Blush;
            RetangleAutoRemove3.Height = height;
            RetangleAutoRemove3.Width = width;
            RetangleAutoRemove3.Opacity = Opacity;
            //Xac định tọa độ
            RetangleAutoRemove3.Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + RetangleAutoRemove3.Width + StartX * 2, -798 + dConvertToTabletY + RetangleAutoRemove3.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(RetangleAutoRemove3);
        }
        //********************************************************************************
        //********************************************************************************
        //Vẽ những hình Chữ nhật auto remove
        Rectangle RetangleAutoRemove4 = new Rectangle();
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRect_AutoRemove4(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(RetangleAutoRemove4);
            RetangleAutoRemove4.Fill = Blush;
            RetangleAutoRemove4.Height = height;
            RetangleAutoRemove4.Width = width;
            RetangleAutoRemove4.Opacity = Opacity;
            //Xac định tọa độ
            RetangleAutoRemove4.Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + RetangleAutoRemove4.Width + StartX * 2, -798 + dConvertToTabletY + RetangleAutoRemove4.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(RetangleAutoRemove4);
            BackgroundDisplay.Children.Remove(RetangleAutoRemove4);
        }
        //********************************************************************************

        Rectangle RetangleAutoRemove = new Rectangle();
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// Rectangle sẽ Auto Remove khi có Rectangle mới
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRectangleAutoRemove(SolidColorBrush Blush, double StartX, double StartY, double width, double height)
        {
            //Rectangle TestRetangle = new Rectangle();
            RetangleAutoRemove.Fill = new SolidColorBrush(Colors.Pink);
            RetangleAutoRemove.Height = height;
            RetangleAutoRemove.Width = width;
            RetangleAutoRemove.Opacity = 0.5;
            //Xac định tọa độ
            RetangleAutoRemove.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + RetangleAutoRemove.Width + StartX * 2, -798 + dConvertToTabletY + RetangleAutoRemove.Height + StartY * 2, 0, 0);
            BackgroundDisplay.Children.Add(RetangleAutoRemove);
        }
        //Bước đột phá, tạo mảng hình chữ nhật auto remove biến này là biến toàn cục
        Rectangle[] Ret_AutoRemove = new Rectangle[10];

        //Set up for rectangle
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Rect_Setup_AutoRemove(int index, SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Ret_AutoRemove[index]);
            Ret_AutoRemove[index] = new Rectangle();
            Ret_AutoRemove[index].Fill = Blush;
            Ret_AutoRemove[index].Height = height;
            Ret_AutoRemove[index].Width = width;
            Ret_AutoRemove[index].Opacity = Opacity;
            //Xac định tọa độ
            Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Ret_AutoRemove[index]);

        }
        //********************************************************************************

        /// <summary>
        /// Khi tác động vào hình chữ nhật thì remove cái cũ và add vị trí mới
        /// màu sắc và độ đục đã được set up rồi
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Rect_Change_AutoRemove(int index, double StartX, double StartY, double width, double height)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Ret_AutoRemove[index]);
            Ret_AutoRemove[index].Height = height;
            Ret_AutoRemove[index].Width = width;
            //Xac định tọa độ
            Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Ret_AutoRemove[index]);

        }
        //********************************************************************************
        //Vẽ string trong map

        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawString(string drawString, double SizeOfText, SolidColorBrush Blush, double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            TextBlock TxtDesign = new TextBlock();
            //chiều dài rộng của khung chứa text
            //TxtDesign.Height = HeightOfBlock;
            //TxtDesign.Width = WidthOfBlock;
            //canh lề, left, right, center
            TxtDesign.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesign.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesign.Margin = 
            //
            //đảo chữ
            TxtDesign.TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            TxtDesign.Text = drawString;
            TxtDesign.FontSize = SizeOfText;
            TxtDesign.FontFamily = new FontFamily("Arial");
            //TxtDesign.FontStyle = "Arial";
            //TxtDesign.FontStretch
            //color text có độ đục
            //TxtDesign.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesign.Foreground = Blush;
            TxtDesign.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesign.Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesign);
        }
        //************************************************************************
        //Vẽ string trong map
        //Có căn lề
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringHaveCanLe(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double WidthOfBlock, double HeightOfBlock)
        {
            //create graphic text block design text
            TextBlock TxtDesign = new TextBlock();
            //chiều dài rộng của khung chứa text
            TxtDesign.Height = HeightOfBlock;
            TxtDesign.Width = WidthOfBlock;
            //canh lề, left, right, center
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            TxtDesign.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesign.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesign.li
            //TxtDesign.Margin = 
            //
            //đảo chữ
            TxtDesign.TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            TxtDesign.Text = drawString;
            TxtDesign.FontSize = SizeOfText;
            TxtDesign.FontFamily = new FontFamily("Arial");
            //TxtDesign.FontStyle = "Arial";
            //TxtDesign.FontStretch
            //color text có độ đục
            //TxtDesign.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesign.Foreground = Blush;
            TxtDesign.Opacity = 1;
            //position of text left, top, right, bottom
            TxtDesign.Margin = new Windows.UI.Xaml.Thickness(StartX, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesign);
        }
        //************************************************************************
        //************************************************************************
        //Vẽ string trong map
        //Có căn lề
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringHaveCanLeTren(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double WidthOfBlock, double HeightOfBlock)
        {
            //create graphic text block design text
            TextBlock TxtDesign = new TextBlock();
            //chiều dài rộng của khung chứa text
            TxtDesign.Height = HeightOfBlock;
            TxtDesign.Width = WidthOfBlock;
            //canh lề, left, right, center
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            TxtDesign.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesign.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesign.li
            //TxtDesign.Margin = 
            //
            //đảo chữ
            TxtDesign.TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            TxtDesign.Text = drawString;
            TxtDesign.FontSize = SizeOfText;
            TxtDesign.FontFamily = new FontFamily("Arial");
            //TxtDesign.FontStyle = "Arial";
            //TxtDesign.FontStretch
            //color text có độ đục
            //TxtDesign.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesign.Foreground = Blush;
            TxtDesign.Opacity = 1;
            //position of text left, top, right, bottom
            TxtDesign.Margin = new Windows.UI.Xaml.Thickness(StartX, StartY - 4, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesign);
        }
        //************************************************************************
        //************************************************************************
        //Vẽ string trong map
        //Có căn lề
        //Để vẽ 1/2 chữ
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringHave1Phan2Chu(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double WidthOfBlock, double HeightOfBlock)
        {
            //create graphic text block design text
            TextBlock TxtDesign = new TextBlock();
            //chiều dài rộng của khung chứa text

            //canh lề, left, right, center
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            TxtDesign.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesign.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesign.TextLineBounds = TextLineBounds.TrimToBaseline;
            TxtDesign.Height = HeightOfBlock;
            TxtDesign.Width = WidthOfBlock;
            //TxtDesign.TextTrimming = TextTrimming.WordEllipsis;
            //TxtDesign.w
            //TxtDesign.Visibility = Visibility.;
            //TxtDesign.Padding = new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
            //TxtDesign.b
            //TxtDesign.Visibility = Visibility.
            //TxtDesign.TextAlignment = string
            //TxtDesign.TextLi
            //drawFormat1.Alignment = StringAlignment.Center;
            // drawFormat1.LineAlignment = StringAlignment.Far;
            TxtDesign.TextAlignment = TextAlignment.Left;
            //TxtDesign.AllowDrop = true;
            //TxtDesign.MaxHeight = 10;
            //TxtDesign.OpticalMarginAlignment = OpticalMarginAlignment.TrimSideBearings;
            //TxtDesign.
            //tDesign.TextAlignment = TextLineBounds.TrimToBaseline;
            //TxtDesign.LineHeight = double.NaN;
            //TxtDesign.
            //TxtDesign.li
            //TxtDesign.Margin = 
            //
            //đảo chữ
            TxtDesign.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesign.Text = drawString;
            TxtDesign.FontSize = SizeOfText;
            TxtDesign.FontFamily = new FontFamily("Arial");
            //TxtDesign.FontStyle = "Arial";
            //TxtDesign.FontStretch
            //color text có độ đục
            //TxtDesign.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesign.Foreground = Blush;
            TxtDesign.Opacity = 1;
            //position of text left, top, right, bottom
            TxtDesign.Margin = new Windows.UI.Xaml.Thickness(StartX - 000, StartY - 00, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesign);
        }
        //************************************************************************
        //************************************************************************
        //************************************************************************
        //Vẽ string trong map
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringCanLePhai(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            TextBlock TxtDesign = new TextBlock();
            //chiều dài rộng của khung chứa text
            TxtDesign.Height = 30.0;
            TxtDesign.Width = 100.0;
            //canh lề, left, right, center
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            TxtDesign.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesign.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesign.TextAlignment = string
            //TxtDesign.TextLi
            //drawFormat1.Alignment = StringAlignment.Center;
            // drawFormat1.LineAlignment = StringAlignment.Far;
            //Canh lề phải
            TxtDesign.TextAlignment = TextAlignment.Right;
            //TxtDesign.LineHeight = double.NaN;
            //TxtDesign.
            //TxtDesign.li
            //TxtDesign.Margin = 
            //
            //đảo chữ
            TxtDesign.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesign.Text = drawString;
            TxtDesign.FontSize = SizeOfText;
            TxtDesign.FontFamily = new FontFamily("Arial");
            //TxtDesign.FontStyle = "Arial";
            //TxtDesign.FontStretch
            //color text có độ đục
            //TxtDesign.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesign.Foreground = Blush;
            TxtDesign.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesign.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesign);
        }
        //************************************************************************

        //************************************************************************
        //************************************************************************
        //Vẽ string Auto remove trong map
        TextBlock TxtDesignAutoRemove = new TextBlock();
        TextBlock TxtDesignAutoRemove1 = new TextBlock();
        TextBlock TxtDesignAutoRemove2 = new TextBlock();
        TextBlock TxtDesignAutoRemove3 = new TextBlock();
        TextBlock TxtDesignAutoRemove4 = new TextBlock();
        TextBlock TxtDesignAutoRemove5 = new TextBlock();
        TextBlock TxtDesignAutoRemove6 = new TextBlock();
        TextBlock TxtDesignAutoRemove7 = new TextBlock();

        TextBlock TxtDesignAutoRemove9 = new TextBlock();
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove);
            //create graphic text block design text
            //TextBlock TxtDesignAutoRemove = new TextBlock();
            //chiều dài rộng của khung chứa text
            TxtDesignAutoRemove.Height = 30.0;
            TxtDesignAutoRemove.Width = 100.0;
            //canh lề, left, right, center
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            TxtDesignAutoRemove.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesignAutoRemove.TextAlignment = string
            //TxtDesignAutoRemove.TextLi
            //drawFormat1.Alignment = StringAlignment.Center;
            // drawFormat1.LineAlignment = StringAlignment.Far;
            //Canh lề phải
            TxtDesignAutoRemove.TextAlignment = TextAlignment.Right;
            //TxtDesignAutoRemove.LineHeight = double.NaN;
            //TxtDesignAutoRemove.
            //TxtDesignAutoRemove.li
            //TxtDesignAutoRemove.Margin = 
            //
            //đảo chữ
            TxtDesignAutoRemove.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove.Text = drawString;
            TxtDesignAutoRemove.FontSize = SizeOfText;
            TxtDesignAutoRemove.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove.FontStyle = "Arial";
            //TxtDesignAutoRemove.FontStretch
            //color text có độ đục
            //TxtDesignAutoRemove.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove.Foreground = Blush;
            TxtDesignAutoRemove.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove);
        }
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove1(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove1);
            //create graphic text block design text
            //TextBlock TxtDesignAutoRemove1 = new TextBlock();
            //chiều dài rộng của khung chứa text
            TxtDesignAutoRemove1.Height = 30.0;
            TxtDesignAutoRemove1.Width = 100.0;
            //canh lề, left, right, center
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            TxtDesignAutoRemove1.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove1.VerticalAlignment = VerticalAlignment.Top;
            //TxtDesignAutoRemove1.TextAlignment = string
            //TxtDesignAutoRemove1.TextLi
            //drawFormat1.Alignment = StringAlignment.Center;
            // drawFormat1.LineAlignment = StringAlignment.Far;
            //Canh lề phải
            TxtDesignAutoRemove1.TextAlignment = TextAlignment.Right;
            //TxtDesignAutoRemove1.LineHeight = double.NaN;
            //TxtDesignAutoRemove1.
            //TxtDesignAutoRemove1.li
            //TxtDesignAutoRemove1.Margin = 
            //
            //đảo chữ
            TxtDesignAutoRemove1.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove1.Text = drawString;
            TxtDesignAutoRemove1.FontSize = SizeOfText;
            TxtDesignAutoRemove1.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove1.FontStyle = "Arial";
            //TxtDesignAutoRemove1.FontStretch
            //color text có độ đục
            //TxtDesignAutoRemove1.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove1.Foreground = Blush;
            TxtDesignAutoRemove1.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove1.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove1);
        }

        //********************************************************
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove2(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove2);
            TxtDesignAutoRemove2.Height = 30.0;
            TxtDesignAutoRemove2.Width = 100.0;
            TxtDesignAutoRemove2.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove2.VerticalAlignment = VerticalAlignment.Top;
            TxtDesignAutoRemove2.TextAlignment = TextAlignment.Right;
            TxtDesignAutoRemove2.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove2.Text = drawString;
            TxtDesignAutoRemove2.FontSize = SizeOfText;
            TxtDesignAutoRemove2.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove2.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove2.Foreground = Blush;
            TxtDesignAutoRemove2.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove2.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove2);
        }
        //********************************************************
        //********************************************************
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove3(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove3);
            TxtDesignAutoRemove3.Height = 30.0;
            TxtDesignAutoRemove3.Width = 100.0;
            TxtDesignAutoRemove3.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove3.VerticalAlignment = VerticalAlignment.Top;
            TxtDesignAutoRemove3.TextAlignment = TextAlignment.Right;
            TxtDesignAutoRemove3.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove3.Text = drawString;
            TxtDesignAutoRemove3.FontSize = SizeOfText;
            TxtDesignAutoRemove3.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove3.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove3.Foreground = Blush;
            TxtDesignAutoRemove3.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove3.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove3);
        }
        //********************************************************
        //********************************************************
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove4(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove4);
            TxtDesignAutoRemove4.Height = 30.0;
            TxtDesignAutoRemove4.Width = 100.0;
            TxtDesignAutoRemove4.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove4.VerticalAlignment = VerticalAlignment.Top;
            TxtDesignAutoRemove4.TextAlignment = TextAlignment.Right;
            TxtDesignAutoRemove4.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove4.Text = drawString;
            TxtDesignAutoRemove4.FontSize = SizeOfText;
            TxtDesignAutoRemove4.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove4.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove4.Foreground = Blush;
            TxtDesignAutoRemove4.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove4.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove4);
        }
        //********************************************************
        //********************************************************
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove5(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove5);
            TxtDesignAutoRemove5.Height = 30.0;
            TxtDesignAutoRemove5.Width = 100.0;
            TxtDesignAutoRemove5.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove5.VerticalAlignment = VerticalAlignment.Top;
            TxtDesignAutoRemove5.TextAlignment = TextAlignment.Right;
            TxtDesignAutoRemove5.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove5.Text = drawString;
            TxtDesignAutoRemove5.FontSize = SizeOfText;
            TxtDesignAutoRemove5.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove5.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove5.Foreground = Blush;
            TxtDesignAutoRemove5.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove5.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove5);
        }
        //********************************************************
        //********************************************************
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove6(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove6);
            TxtDesignAutoRemove6.Height = 30.0;
            TxtDesignAutoRemove6.Width = 100.0;
            TxtDesignAutoRemove6.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove6.VerticalAlignment = VerticalAlignment.Top;
            TxtDesignAutoRemove6.TextAlignment = TextAlignment.Right;
            TxtDesignAutoRemove6.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove6.Text = drawString;
            TxtDesignAutoRemove6.FontSize = SizeOfText;
            TxtDesignAutoRemove6.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove6.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove6.Foreground = Blush;
            TxtDesignAutoRemove6.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove6.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove6);
        }
        //********************************************************
        //********************************************************
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove7(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove7);
            TxtDesignAutoRemove7.Height = 30.0;
            TxtDesignAutoRemove7.Width = 100.0;
            TxtDesignAutoRemove7.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove7.VerticalAlignment = VerticalAlignment.Top;
            TxtDesignAutoRemove7.TextAlignment = TextAlignment.Right;
            TxtDesignAutoRemove7.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove7.Text = drawString;
            TxtDesignAutoRemove7.FontSize = SizeOfText;
            TxtDesignAutoRemove7.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove7.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove7.Foreground = Blush;
            TxtDesignAutoRemove7.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove7.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove7);
        }
        //********************************************************
        //********************************************************
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void DrawStringAutoRemove8(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            TextBlock TxtDesignAutoRemove8 = new TextBlock();
            //BackgroundDisplay.Children.Remove(TxtDesignAutoRemove8);
            TxtDesignAutoRemove8.Height = 30.0;
            TxtDesignAutoRemove8.Width = 100.0;
            TxtDesignAutoRemove8.HorizontalAlignment = HorizontalAlignment.Left;
            TxtDesignAutoRemove8.VerticalAlignment = VerticalAlignment.Top;
            TxtDesignAutoRemove8.TextAlignment = TextAlignment.Right;
            TxtDesignAutoRemove8.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            TxtDesignAutoRemove8.Text = drawString;
            TxtDesignAutoRemove8.FontSize = SizeOfText;
            TxtDesignAutoRemove8.FontFamily = new FontFamily("Arial");
            //TxtDesignAutoRemove8.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            TxtDesignAutoRemove8.Foreground = Blush;
            TxtDesignAutoRemove8.Opacity = Opacity;
            //position of text left, top, right, bottom
            TxtDesignAutoRemove8.Margin = new Windows.UI.Xaml.Thickness(StartX - 70, StartY, 0, 0);
            BackgroundDisplay.Children.Add(TxtDesignAutoRemove8);
        }
        //********************************************************
        public void Speed_ModeAutoRemove(string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity, int index)
        {
            switch (index)
            {
                case 1:
                    {
                        DrawStringAutoRemove1(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
                case 2:
                    {
                        DrawStringAutoRemove2(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
                case 3:
                    {
                        DrawStringAutoRemove3(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
                case 4:
                    {
                        DrawStringAutoRemove4(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
                case 5:
                    {
                        DrawStringAutoRemove5(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
                case 6:
                    {
                        DrawStringAutoRemove6(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
                case 7:
                    {
                        DrawStringAutoRemove7(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
                case 8:
                    {
                        DrawStringAutoRemove8(drawString, SizeOfText, Blush,
                        StartX, StartY, Opacity);
                        break;
                    }
            }
        }
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_Compass_Display_Angle = new TextBlock[1];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Compass_Setup_Display_Angle(int index, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity, double HeightOfBlock, double WidthOfBlock)
        {
            //create graphic text block design text
            //TextBlock Tb_Compass_Display_Angle[index] = new TextBlock();

            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Compass_Display_Angle[index]);
            Tb_Compass_Display_Angle[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            Tb_Compass_Display_Angle[index].Height = HeightOfBlock;
            Tb_Compass_Display_Angle[index].Width = WidthOfBlock;
            Tb_Compass_Display_Angle[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Compass_Display_Angle[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Compass_Display_Angle[index].Margin = 
            //
            //đảo chữ
            Tb_Compass_Display_Angle[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Compass_Display_Angle[index].Text = drawString;
            Tb_Compass_Display_Angle[index].FontSize = SizeOfText;
            Tb_Compass_Display_Angle[index].TextAlignment = TextAlignment.Center;
            Tb_Compass_Display_Angle[index].FontFamily = new FontFamily("Arial");
            //Tb_Compass_Display_Angle[index].FontStyle = "Arial";
            //Tb_Compass_Display_Angle[index].FontStretch
            //color text có độ đục
            //Tb_Compass_Display_Angle[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Compass_Display_Angle[index].Foreground = Blush;
            Tb_Compass_Display_Angle[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Compass_Display_Angle[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Compass_Display_Angle[index]);
        }
        //**********************************************************************************************
        public void Compass_Display_Angle_run(int index, string drawString,
    double StartX, double StartY)
        {
            //create graphic text block design text
            //TextBlock Tb_Compass_Display_Angle[index] = new TextBlock();

            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Compass_Display_Angle[index]);
            //Tb_Compass_Display_Angle[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Compass_Display_Angle[index].Height = HeightOfBlock;
            //Tb_Compass_Display_Angle[index].Width = WidthOfBlock;
            //Tb_Compass_Display_Angle[index].HorizontalAlignment = HorizontalAlignment.Left;
            //Tb_Compass_Display_Angle[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Compass_Display_Angle[index].Margin = 
            //
            //đảo chữ
            //Tb_Compass_Display_Angle[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Compass_Display_Angle[index].Text = drawString;
            //Tb_Compass_Display_Angle[index].FontSize = SizeOfText;
            //Tb_Compass_Display_Angle[index].TextAlignment = TextAlignment.Center;
            //Tb_Compass_Display_Angle[index].FontFamily = new FontFamily("Arial");
            //Tb_Compass_Display_Angle[index].FontStyle = "Arial";
            //Tb_Compass_Display_Angle[index].FontStretch
            //color text có độ đục
            //Tb_Compass_Display_Angle[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //Tb_Compass_Display_Angle[index].Foreground = Blush;
            //Tb_Compass_Display_Angle[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Compass_Display_Angle[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Compass_Display_Angle[index]);
        }
        //**********************************************************************************************
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_Compass = new TextBlock[20];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Compass_SetupString_AutoRemove(int index, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_Compass[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Compass[index].Height = HeightOfBlock;
            //Tb_Compass[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Compass[index]);
            Tb_Compass[index] = new TextBlock();
            Tb_Compass[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Compass[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Compass[index].Margin = 
            //
            //đảo chữ
            Tb_Compass[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Compass[index].Text = drawString;
            Tb_Compass[index].FontSize = SizeOfText;
            Tb_Compass[index].FontFamily = new FontFamily("Arial");
            //Tb_Compass[index].FontStyle = "Arial";
            //Tb_Compass[index].FontStretch
            //color text có độ đục
            //Tb_Compass[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Compass[index].Foreground = Blush;
            Tb_Compass[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Compass[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Compass[index]);
        }
        //**********************************************************************************************
        public void Compass_SetupStr_AutoRem_run(int index, string drawString, double StartX, double StartY)
        {
            //create graphic text block design text
            //TextBlock Tb_Compass[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Compass[index].Height = HeightOfBlock;
            //Tb_Compass[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Compass[index]);
            //Tb_Compass[index] = new TextBlock();
            //Tb_Compass[index].HorizontalAlignment = HorizontalAlignment.Left;
            //Tb_Compass[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Compass[index].Margin = 
            //
            //đảo chữ
            //Tb_Compass[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Compass[index].Text = drawString;
            //Tb_Compass[index].FontSize = SizeOfText;
            //Tb_Compass[index].FontFamily = new FontFamily("Arial");
            //Tb_Compass[index].FontStyle = "Arial";
            //Tb_Compass[index].FontStretch
            //color text có độ đục
            //Tb_Compass[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //Tb_Compass[index].Foreground = Blush;
            //Tb_Compass[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Compass[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Compass[index]);
        }
        //**********************************************************************************************
        /// <summary>
        /// Remove chỗi string cũ
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Compass_ChangeString_AutoRemove(int index, string drawString,
            double StartX, double StartY)
        {
            //create graphic text block design text
            //TextBlock Tb_Compass[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Compass[index].Height = HeightOfBlock;
            //Tb_Compass[index].Width = WidthOfBlock;
            //canh lề, left, right, center

            //Tb_Compass[index].Margin = 
            //

            BackgroundDisplay.Children.Remove(Tb_Compass[index]);
            Tb_Compass[index].Text = drawString;


            //Tb_Compass[index].FontStyle = "Arial";
            //Tb_Compass[index].FontStretch
            //color text có độ đục
            //Tb_Compass[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));

            //position of text left, top, right, bottom
            Tb_Compass[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Compass[index]);
        }
        //*********************************************************************************************
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_Alt = new TextBlock[10];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Alt_SetupString_AutoRemove(int index, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_Alt[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Alt[index].Height = HeightOfBlock;
            //Tb_Alt[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Alt[index]);
            Tb_Alt[index] = new TextBlock();
            Tb_Alt[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Alt[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Alt[index].Margin = 
            //
            //đảo chữ
            Tb_Alt[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Alt[index].Text = drawString;
            Tb_Alt[index].FontSize = SizeOfText;
            Tb_Alt[index].FontFamily = new FontFamily("Arial");
            //Tb_Alt[index].FontStyle = "Arial";
            //Tb_Alt[index].FontStretch
            //color text có độ đục
            //Tb_Alt[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Alt[index].Foreground = Blush;
            Tb_Alt[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Alt[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Alt[index]);
        }
        public void Alt_Setup_Image_AutoRemove(int index, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_Alt[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Alt[index].Height = HeightOfBlock;
            //Tb_Alt[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            //BackgroundDisplay.Children.Remove(Tb_Alt[index]);
            Tb_Alt[index] = new TextBlock();
            Tb_Alt[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Alt[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Alt[index].Margin = 
            //
            //đảo chữ
            Tb_Alt[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Alt[index].Text = drawString;
            Tb_Alt[index].FontSize = SizeOfText;
            Tb_Alt[index].FontFamily = new FontFamily("Arial");
            //Tb_Alt[index].FontStyle = "Arial";
            //Tb_Alt[index].FontStretch
            //color text có độ đục
            //Tb_Alt[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Alt[index].Foreground = Blush;
            Tb_Alt[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_Alt[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Alt[index]);
        }
        //**********************************************************************************************
        /// <summary>
        /// Remove chỗi string cũ
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Alt_ChangeString_AutoRemove(int index, string drawString,
            double StartX, double StartY)
        {
            //create graphic text block design text
            //TextBlock Tb_Alt[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Alt[index].Height = HeightOfBlock;
            //Tb_Alt[index].Width = WidthOfBlock;
            //canh lề, left, right, center

            //Tb_Alt[index].Margin = 
            //

            BackgroundDisplay.Children.Remove(Tb_Alt[index]);
            Tb_Alt[index].Text = drawString;


            //Tb_Alt[index].FontStyle = "Arial";
            //Tb_Alt[index].FontStretch
            //color text có độ đục
            //Tb_Alt[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));

            //position of text left, top, right, bottom
            Tb_Alt[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Alt[index]);
        }
        //*********************************************************************************************
        //Draw Air Speed
        //Vị trí của mặt phân cách đường chân trời ở trục x
        Int16 Blue_BackGround = 250;
        void AirSpeed_Draw_Test(double Air_Speed)
        {
            //Graphics formGraphic = this.CreateGraphics();
            //Create pens.
            //Pen redPen = new Pen(Color.Red, 3);
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            // Draw lines between original points to screen.
            //formGraphic.DrawLines(redPen, curvePoints);
            //Các bút vẽ cần thiết
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            Int16 PoinStart_X = 60, PoinStart_Y = 50, Width = 8, Height = 250;
            //Draw BackGround độ đục là 1
            //Delete cái gì vẽ trước đó
            //Ngày 16/12/2015 15h22 đã test ok
            FillRectangleHaveOpacity(BlushRectangle1, PoinStart_X - 50, PoinStart_Y - 8, 50 + Width,
                Blue_BackGround - (PoinStart_Y - 8), 1.0);
            FillRectangleHaveOpacity(BlushRectangle2, PoinStart_X - 50, Blue_BackGround, 50 + Width,
                (PoinStart_Y + Height + 5) - Blue_BackGround, 1);
            //Ve cot cao hinh chu nhat
            //Ngày 16/12/2015 15h22 đã test ok
            FillRectangle(BlushRectangle3, PoinStart_X, PoinStart_Y, Width, Height);
            //Ve duong vien mau trang
            //Chưa vẽ được
            Point[] curvePoints = { new Point(PoinStart_X + Width, PoinStart_Y - 12), new Point(PoinStart_X + Width - 50,
                PoinStart_Y - 12), new Point(PoinStart_X + Width - 50 , PoinStart_Y + Height + 8),
                                  new Point(PoinStart_X + Width, PoinStart_Y + Height + 8)};


            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePoints);



            DrawLine(BlushOfLine1, 1, PoinStart_X + Width, PoinStart_Y - 12,
                PoinStart_X + Width, PoinStart_Y + Height + 8);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Ngày 16/12/2015 15h22 đã test ok
            //Cỡ chữ 16 bên map là cỡ chữ 12 bên System.Drawing
            //
            Int16 Index_Resolution = 60;
            int indexMode = 0;
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height; AirSpeed_Index += (double)Height / 6)
            {
                DrawLine(BlushOfLine1, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X + Width - 15, AirSpeed_Index - 1);
                //Ghi chu chi do phan giai
                //Ngày 16/12/2015 15h22 đã test ok
                indexMode++;
                Speed_ModeAutoRemove(((Int16)Air_Speed / 10 * 10 - 30 + Index_Resolution).ToString(),
                    16, BlushOfString1, PoinStart_X - 40, AirSpeed_Index - 10, 1, indexMode);
                Index_Resolution -= 10;
            }
            for (double AirSpeed_Index = PoinStart_Y + Height / 12; AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / 6)
            {
                DrawLine(BlushOfLine1, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X, AirSpeed_Index - 1);
            }
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            //*************************************************************************
            //Ve Cho mau den de ghi Airspeed hang tram và hàng chục
            //Ngày 16/12/2015 15h52 đã test ok
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(Int16)(PoinStart_X + 15)
             */
            Int16 I16FullScale = 60, So_Khoang_Chia = 6;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            Int16 Config_Position = 12, DoRongVungDen = 26, i16StartStrAxisX;
            i16StartStrAxisX = (Int16)(PoinStart_X - 39);
            FillRectangleHaveOpacity(BlushRectangle4, i16StartStrAxisX, PoinStart_Y - Config_Position +
                (30 - (Int16)Air_Speed % 10) * Height / 60, DoRongVungDen, Config_Position * 2, 1);

            //Vẽ mũi tên
            //Ngày 16/12/2015 15h52 đã test ok
            int x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX + DoRongVungDen;
            y1 = PoinStart_Y - Config_Position +
                (30 - (Int16)Air_Speed % 10) * Height / 60;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X + Width;
            y3 = (y1 + y2) / 2;
            Draw_TriAngle_Var(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)Air_Speed / 100
            //Ngày 16/12/2015 15h56 đã test ok
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)Air_Speed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)Air_Speed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)Air_Speed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */
            Int16 SizeOfString = 24;
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            //drawFont = new Font("Arial", SizeOfString);
            //-2 là độ dời chữ vào trong thích hợp
            DrawStringCanLePhai(((Int16)Air_Speed / 10).ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 3,
                                PoinStart_Y - 14 + (30 - (Int16)Air_Speed % 10) * Height / I16FullScale, 1);
            //Ve mau den hang don vi
            //Ngày 16/12/2015 15h55 đã test ok
            /*Vị trí bắt đầu: i16StartStrAxisX + DoRongVungDen
             * Vị trí trên trục y: (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Lùi trục y 1 khoang SizeOfString để chữ trung tâm nằm giữa màu đen
             * Độ cao màu đen viết được 2 số theo chiều cao: SizeOfString * 2
             * Độ rông vạch đen bằng 26
             */
            //Chú ý nếu size chữ là 24 thì chiều cao chữ nhỏ hơn 24
            FillRectangleHaveOpacity(BlushRectangle4, i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
            (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 10) * Height / I16FullScale, 13, SizeOfString * 2, 1);
            //ghi chu len mau den làm tròn đến hàng đơn vị
            /* Vị tri chu bat dau i16StartStrAxisX + DoRongVungDen
             * Lấy chữ số hàng chục: ((Int16)Air_Speed % 100 / 10)
             * Doi trung tâm chữ về bên trái DoiChu = 3 đơn vị
             * Lệnh DrawString chữ bắt đầu tại PointX - Font_X / 4 = PointX - 12/ 4 = PointX - 3;
             * Cách xác định vị trí hàng đơn vị:
             * Vị trí hàng trăm PoinStart_Y - 12 + ((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Cũng chính là vị trí hàng chục nếu hàng đơn vị bằng 0, ta chỉ thể hiện 3 số hàng chục kế nhau tại vạch màu đen
             * Nếu hàng đơn vị khác 0 ta sẽ dịch chuển vị trí hàng chục đi xuống bằng cách tăng y
             * Nếu hàng đơn vị là 10 thì ta đi được 1 chữ số 16 đơn vị theo trục y
             * Nên nếu dư a đơn vị ((Int16)Air_Speed % 10) ta dịch 1 khoảng (Int16)Air_Speed % 10 * 16 / 10)
             * 
             */
            /********************************************************************
             * Viết chữ trong phạm vi cho phép
             *     e.Graphics.DrawString(drawString, drawFont, drawBrush, drawRect, drawFormat);
             *     RectangleF drawRect = new RectangleF(x, y, width, height);
             * Điểm bắt đầu màu đen nhỏ là (i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
             * (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale)
             * Điểm kết thúc (i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
             * (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale + 26)
             */

            //Còn bên Notepad++

            Int16 StartOfHChuc, i16DoiChu = 3;
            StartOfHChuc = (Int16)(i16StartStrAxisX + DoRongVungDen - i16DoiChu);
            //drawFont = new Font("Arial", SizeOfString);
            //Vị trí bắt đầu của chuỗi số chục đơn vị StartStrY
            /*Bắt đầu màu đen (StartBlackX, StartBlackY)
             * Kết thúc màu đen (StartBlackX, EndBlackY)
             * 
             */
            //Ngày 03/10/2015
            Int32 StartBlackX = StartOfHChuc + 4;
            Int32 StartBlackY = PoinStart_Y - SizeOfString +
                (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 10) * Height / I16FullScale;
            Int32 EndBlackY = PoinStart_Y - SizeOfString +
                (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 10) * Height / I16FullScale + SizeOfString * 2;
            Int32 StartStrY = PoinStart_Y - 8 + ((I16FullScale / 2)
                - (Int16)Air_Speed % 10) * Height / I16FullScale + (Int16)(Air_Speed * 10) % 10 * 16 / 10;
            //Vẽ hình chữ nhật bao chữ lại
            //Vì canh lề trên nên ta dời hình chữ nhật lên trên 4 đơn vị nên có  số 4 xuất hiện
            //Rectangle drawRect1 = new Rectangle(StartBlackX, StartStrY - 4, 12, (EndBlackY - StartStrY + 4));
            //formGraphic.DrawRectangle(Pens.Red, drawRect1);

            // Set format of string.
            //StringFormat drawFormat = new StringFormat();
            //drawFormat.Alignment = StringAlignment.Center;
            //drawFormat.LineAlignment = StringAlignment.Near;
            //formGraphic.DrawString(((Int16)Air_Speed % 10).ToString(), drawFont, drawBrush,
            //   drawRect1, drawFormat);
            //Code Edit bên Map
            //Ngày 16/12/2015 15h56 đã test ok
            //Đã đúng chữ số hàng đơn vị trung tâm

            DrawStringHaveCanLe(((Int16)Air_Speed % 10).ToString(),
                SizeOfString, BlushOfString1, StartBlackX, StartStrY - 6, 12, (EndBlackY - StartStrY + 4));
            //formGraphic.DrawString(0.ToString(), drawFont, drawBrush, StartOfHChuc + 12,
            //    PoinStart_Y - 12 + ((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale
            //   + (Int16)Air_Speed % 10 * 16 / 10);

            /*Viết Chữ số hàng đơn vị trên trung tâm 10 đơn vị
             * Nên trục y sẽ được trừ đi 16 so với trục y của hàng chục trung tâm
             * Số 16 là độ dời 1 chữ số theo chiều ca0
             * Canh lề dưới có chặn trên là StartBlackY
             * Nên bắt đầu của chuỗi là StartBlackY
             * Vì canh lề trên nên phải dời xuống dưới 7 đơn vị để vị trí chữ thích hợp
             */
            // Set format of string.
            //Thay code bên System.Drawing
            /*
            StringFormat drawFormat1 = new StringFormat();
            drawFormat1.Alignment = StringAlignment.Center;
            drawFormat1.LineAlignment = StringAlignment.Far;
            drawRect1 = new Rectangle(StartBlackX, StartBlackY, 12, ((StartStrY + 7 - StartBlackY)));
            formGraphic.DrawString(((Int16)(Air_Speed + 1) % 10).ToString(), drawFont, drawBrush,
                drawRect1, drawFormat1);
                */
            //Code Edit bên Map
            //Khong can le duoi được
            DrawStringHaveCanLeTren(((Int16)(Air_Speed + 1) % 10).ToString(),
                SizeOfString, BlushOfString1, StartBlackX, StartBlackY, 16, (StartStrY + 7 - StartBlackY));
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //Nếu hàng đơn vị nhỏ hơn 6
            /*Viết Chữ số hàng chục dưới trung tâm 10 đơn vị
             * Nên trục y sẽ được cộng đi 16 so với trục y của hàng chục trung tâm
             * Số 16 là đô dời 1 chữ số theo chiều cao
             */
            if (((Int16)(Air_Speed * 10) % 10) <= 5)
            {
                //drawRect1 = new Rectangle(StartBlackX, StartStrY - 4 + 16, 12, (EndBlackY - (StartStrY - 4 + 16)));
                //formGraphic.DrawString(((Int16)(Air_Speed - 1) % 10).ToString(), drawFont, drawBrush,
                //    drawRect1, drawFormat);
                //Code Edit bên Map
                //Ngay 18/12/2015 15h00 đã test ok
                DrawStringHaveCanLe(((Int16)(Air_Speed - 1) % 10).ToString(),
                    SizeOfString, BlushOfString1, StartBlackX, StartStrY - 4 + SizeOfString * 4 / 5, 16, (EndBlackY - (StartStrY - 4 + SizeOfString * 4 / 5)));

            }
            else
            //Nếu hàng đơn vị lớn hơn 6
            /*Viết Chữ số hàng chục trên trung tâm 20 đơn vị
             * Nên trục y sẽ được trừ đi 32 so với trục y của hàng chục trung tâm
             * Số 32 là đô dời 1 chữ số theo chiều cao
             */
            {
                //Số -16 là đọ dời 1 chữ số so với chữ dưới nó
                //drawRect1 = new Rectangle(StartBlackX, StartBlackY, 12, ((StartStrY + 7 - 16 - StartBlackY)));
                //formGraphic.DrawString(((Int16)(Air_Speed + 2) % 10).ToString(), drawFont, drawBrush,
                //    drawRect1, drawFormat1);
                //Code Edit bên Map
                DrawStringHaveCanLe(((Int16)(Air_Speed + 2) % 10).ToString(),
                    SizeOfString, BlushOfString1, StartBlackX, StartBlackY, 12, (StartStrY + 7 - 16 - StartBlackY));
            }






        }
        //****************************************************************************************
        //************************************************************************

        //********************************************************
        //*****************************************************************
        //Draw Air Speed
        //Vị trí của mặt phân cách đường chân trời ở trục x
        //Int16 Blue_BackGround = 250;
        //double Speed_Old;
        //
        /// <summary>
        /// Ngày 18/12/2015 Vì khó quá nên làm cái đơn giản
        /// Không giống như đồng hồ của xe máy
        /// Chỉ gọi 1 lần khi setting
        /// PoinStart_X, PoinStart_Y tọa độ hình chữ nhật
        /// </summary>
        /// <param name="Air_Speed"></param>
        void Speed_Setup(double Air_Speed, double PoinStart_X, double PoinStart_Y)
        {
            //Test ok lúc 0h38 ngày 19/12/2015
            //Graphics formGraphic = this.CreateGraphics();
            //Create pens.
            //Pen redPen = new Pen(Color.Red, 3);
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            // Draw lines between original points to screen.
            //formGraphic.DrawLines(redPen, curvePoints);
            //Các bút vẽ cần thiết
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);

            //Tọa độ của Hình vẽ
            //Width, Height là độ rộng và cao của vạch xanh
            double Width = 8, Height = 250;
            //Viết chữ Speed + đơn vị km/h
            DrawString("Speed (Km/h)", 30, new SolidColorBrush(Colors.Green), PoinStart_X - 110, PoinStart_Y + Height + 5, 0.8);
            //Draw BackGround độ đục là 1

            //Delete cái gì vẽ trước đó
            //Ngày 16/12/2015 15h22 đã test ok
            //if (Air_Speed.ToString().Length >= Speed_Old.ToString().Length)
            {
                FillRectangleHaveOpacity(new SolidColorBrush(Colors.Blue), PoinStart_X - 80, PoinStart_Y - 8, 80 + Width,
                    Blue_BackGround - (PoinStart_Y - 8), 0.2);
                FillRectangleHaveOpacity(BlushRectangle2, PoinStart_X - 80, Blue_BackGround, 80 + Width,
                    (PoinStart_Y + Height + 5) - Blue_BackGround, 0.2);
                //bổ sung ngày 18/12/2015
                //Độ rộng Background phụ thuộc vào số ký tự trong Speed
                /*
                if (Air_Speed.ToString().Length > 3)
                {
                    FillRect_AutoRemove1(BlushRectangle1, PoinStart_X - 52 - (Air_Speed.ToString().Length - 3) * 14, PoinStart_Y - 8,
                        52 + (Air_Speed.ToString().Length - 3) * 14 + Width, Blue_BackGround - (PoinStart_Y - 8), 0.2);
                    FillRect_AutoRemove2(BlushRectangle2, PoinStart_X - 52 - (Air_Speed.ToString().Length - 3) * 14, Blue_BackGround,
                        52 + (Air_Speed.ToString().Length - 3) * 14 + Width, (PoinStart_Y + Height + 5) - Blue_BackGround, 0.2);
                }
                */
            }
            //else
            /*
            {
                FillRectangleHaveOpacity(BlushRectangle1, PoinStart_X - 52, PoinStart_Y - 8, 52 + Width,
                    Blue_BackGround - (PoinStart_Y - 8), 1);
                FillRectangleHaveOpacity(BlushRectangle2, PoinStart_X - 52, Blue_BackGround, 52 + Width,
                    (PoinStart_Y + Height + 5) - Blue_BackGround, 1);
                //bổ sung ngày 18/12/2015
                //Độ rộng Background phụ thuộc vào số ký tự trong Speed
                if (Speed_Old.ToString().Length >= 3)
                {
                    FillRectangleHaveOpacity(BlushRectangle1, PoinStart_X - 52 - (Speed_Old.ToString().Length - 3) * 14, PoinStart_Y - 8,
                        52 + (Speed_Old.ToString().Length - 3) * 14 + Width, Blue_BackGround - (PoinStart_Y - 8), 1);
                    FillRectangleHaveOpacity(BlushRectangle2, PoinStart_X - 52 - (Speed_Old.ToString().Length - 3) * 14, Blue_BackGround,
                        52 + (Speed_Old.ToString().Length - 3) * 14 + Width, (PoinStart_Y + Height + 5) - Blue_BackGround, 1);
                }
            }
            */
            //Speed_Old = Air_Speed;
            //Ve cot cao hinh chu nhat
            //Ngày 16/12/2015 15h22 đã test ok
            FillRectangle(BlushRectangle3, PoinStart_X, PoinStart_Y, Width, Height);
            //Ve duong vien mau trang
            //Chưa vẽ được
            Point[] curvePoints = { new Point(PoinStart_X + Width, PoinStart_Y - 12), new Point(PoinStart_X + Width - 50,
                PoinStart_Y - 12), new Point(PoinStart_X + Width - 50 , PoinStart_Y + Height + 8),
                                  new Point(PoinStart_X + Width, PoinStart_Y + Height + 8)};


            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePoints);



            DrawLine(BlushOfLine1, 1, PoinStart_X + Width, PoinStart_Y - 12,
                PoinStart_X + Width, PoinStart_Y + Height + 8);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Ngày 16/12/2015 15h22 đã test ok
            //Cỡ chữ 16 bên map là cỡ chữ 12 bên System.Drawing
            //
            Int16 Index_Resolution = 60;
            int indexMode = 0;
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height; AirSpeed_Index += (double)Height / 6)
            {
                DrawLine(BlushOfLine1, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X + Width - 15, AirSpeed_Index - 1);
                //Ghi chu chi do phan giai
                //Ngày 16/12/2015 15h22 đã test ok
                indexMode++;
                Speed_ModeAutoRemove(((Int16)Air_Speed / 10 * 10 - 30 + Index_Resolution).ToString(),
                    16, BlushOfString1, PoinStart_X - 40, AirSpeed_Index - 10, 1, indexMode);
                Index_Resolution -= 10;
            }
            for (double AirSpeed_Index = PoinStart_Y + Height / 12; AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / 6)
            {
                DrawLine(BlushOfLine1, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X, AirSpeed_Index - 1);
            }
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            //*************************************************************************
            //Ve Cho mau den de ghi Airspeed
            //Ngày 16/12/2015 15h52 đã test ok
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(Int16)(PoinStart_X + 15)
             */
            double SizeOfString = 24;
            double I16FullScale = 60, So_Khoang_Chia = 6;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            //1 ký tự rộng = SizeOfString * 3 / 4;
            double Config_Position = 12, i16StartStrAxisX;
            //double DoRongVungDen = Air_Speed.ToString().Length * (SizeOfString * 3 / 4);
            double DoRongVungDen = 26;
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = Air_Speed.ToString().Length * (SizeOfString * 0.6);
            if (Air_Speed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (Air_Speed.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (Air_Speed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Hình chữ nhật được căn lề phải
            i16StartStrAxisX = (PoinStart_X - 41);
            double StartXRect = (PoinStart_X - 10);
            FillRect_AutoRemove3(BlushRectangle4, StartXRect - DoRongVungDenRect, PoinStart_Y - Config_Position - 1 +
                (30 - Air_Speed % 10) * Height / 60, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            //Ngày 16/12/2015 15h52 đã test ok
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX + DoRongVungDen;
            y1 = PoinStart_Y - Config_Position +
                (30 - Air_Speed % 10) * Height / 60;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X + Width;
            y3 = (y1 + y2) / 2;
            Draw_TriAngle_Var(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)Air_Speed / 100
            //Ngày 16/12/2015 15h56 đã test ok
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)Air_Speed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)Air_Speed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)Air_Speed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            //drawFont = new Font("Arial", SizeOfString);
            //-2 là độ dời chữ vào trong thích hợp
            DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
                                PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);
            //Ve mau den hang don vi
            //Ngày 16/12/2015 15h55 đã test ok
            /*Vị trí bắt đầu: i16StartStrAxisX + DoRongVungDen
             * Vị trí trên trục y: (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Lùi trục y 1 khoang SizeOfString để chữ trung tâm nằm giữa màu đen
             * Độ cao màu đen viết được 2 số theo chiều cao: SizeOfString * 2
             * Độ rông vạch đen bằng 26
             */
            //Chú ý nếu size chữ là 24 thì chiều cao chữ nhỏ hơn 24

            //FillRectangleHaveOpacity(BlushRectangle4, i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
            //(Int16)((I16FullScale / 2) - (Int16)Air_Speed % 10) * Height / I16FullScale, 13, SizeOfString * 2, 1);

            //ghi chu len mau den làm tròn đến hàng đơn vị
            /* Vị tri chu bat dau i16StartStrAxisX + DoRongVungDen
             * Lấy chữ số hàng chục: ((Int16)Air_Speed % 100 / 10)
             * Doi trung tâm chữ về bên trái DoiChu = 3 đơn vị
             * Lệnh DrawString chữ bắt đầu tại PointX - Font_X / 4 = PointX - 12/ 4 = PointX - 3;
             * Cách xác định vị trí hàng đơn vị:
             * Vị trí hàng trăm PoinStart_Y - 12 + ((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Cũng chính là vị trí hàng chục nếu hàng đơn vị bằng 0, ta chỉ thể hiện 3 số hàng chục kế nhau tại vạch màu đen
             * Nếu hàng đơn vị khác 0 ta sẽ dịch chuển vị trí hàng chục đi xuống bằng cách tăng y
             * Nếu hàng đơn vị là 10 thì ta đi được 1 chữ số 16 đơn vị theo trục y
             * Nên nếu dư a đơn vị ((Int16)Air_Speed % 10) ta dịch 1 khoảng (Int16)Air_Speed % 10 * 16 / 10)
             * 
             */
            /********************************************************************
             * Viết chữ trong phạm vi cho phép
             *     e.Graphics.DrawString(drawString, drawFont, drawBrush, drawRect, drawFormat);
             *     RectangleF drawRect = new RectangleF(x, y, width, height);
             * Điểm bắt đầu màu đen nhỏ là (i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
             * (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale)
             * Điểm kết thúc (i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
             * (Int16)((I16FullScale / 2) - (Int16)Air_Speed % 100) * Height / I16FullScale + 26)
             */

            //Còn bên Notepad++
            //Khó quá bỏ qua

        }
        //*************************************************************************
        void Speed_Draw_Speed(double Air_Speed, double PoinStart_X, double PoinStart_Y)
        {
            //Test ok lúc 0h38 ngày 19/12/2015
            //Graphics formGraphic = this.CreateGraphics();
            //Create pens.
            //Pen redPen = new Pen(Color.Red, 3);
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            // Draw lines between original points to screen.
            //formGraphic.DrawLines(redPen, curvePoints);
            //Các bút vẽ cần thiết
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);

            //Tọa độ của Hình vẽ
            //Width, Height là độ rộng và cao của vạch xanh
            double Width = 8, Height = 250;
            //Draw BackGround độ đục là 1

            //Delete cái gì vẽ trước đó
            //Ngày 16/12/2015 15h22 đã test ok
            //if (Air_Speed.ToString().Length >= Speed_Old.ToString().Length)
            {

                //bổ sung ngày 18/12/2015
                //Độ rộng Background phụ thuộc vào số ký tự trong Speed
                /*
                if (Air_Speed.ToString().Length > 3)
                {
                    FillRect_AutoRemove1(BlushRectangle1, PoinStart_X - 52 - (Air_Speed.ToString().Length - 3) * 14, PoinStart_Y - 8,
                        52 + (Air_Speed.ToString().Length - 3) * 14 + Width, Blue_BackGround - (PoinStart_Y - 8), 0.2);
                    FillRect_AutoRemove2(BlushRectangle2, PoinStart_X - 52 - (Air_Speed.ToString().Length - 3) * 14, Blue_BackGround,
                        52 + (Air_Speed.ToString().Length - 3) * 14 + Width, (PoinStart_Y + Height + 5) - Blue_BackGround, 0.2);
                }
                */
            }
            //else
            /*
            {
                FillRectangleHaveOpacity(BlushRectangle1, PoinStart_X - 52, PoinStart_Y - 8, 52 + Width,
                    Blue_BackGround - (PoinStart_Y - 8), 1);
                FillRectangleHaveOpacity(BlushRectangle2, PoinStart_X - 52, Blue_BackGround, 52 + Width,
                    (PoinStart_Y + Height + 5) - Blue_BackGround, 1);
                //bổ sung ngày 18/12/2015
                //Độ rộng Background phụ thuộc vào số ký tự trong Speed
                if (Speed_Old.ToString().Length >= 3)
                {
                    FillRectangleHaveOpacity(BlushRectangle1, PoinStart_X - 52 - (Speed_Old.ToString().Length - 3) * 14, PoinStart_Y - 8,
                        52 + (Speed_Old.ToString().Length - 3) * 14 + Width, Blue_BackGround - (PoinStart_Y - 8), 1);
                    FillRectangleHaveOpacity(BlushRectangle2, PoinStart_X - 52 - (Speed_Old.ToString().Length - 3) * 14, Blue_BackGround,
                        52 + (Speed_Old.ToString().Length - 3) * 14 + Width, (PoinStart_Y + Height + 5) - Blue_BackGround, 1);
                }
            }
            */
            //Speed_Old = Air_Speed;
            //Ve cot cao hinh chu nhat
            //Ngày 16/12/2015 15h22 đã test ok
            //FillRectangle(BlushRectangle3, PoinStart_X, PoinStart_Y, Width, Height);
            //Ve duong vien mau trang
            //Chưa vẽ được



            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePoints);



            //DrawLine(BlushOfLine1, 1, PoinStart_X + Width, PoinStart_Y - 12,
            //    PoinStart_X + Width, PoinStart_Y + Height + 8);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Ngày 16/12/2015 15h22 đã test ok
            //Cỡ chữ 16 bên map là cỡ chữ 12 bên System.Drawing
            //

            //Viet so len do phan giai
            //Ngày 16/12/2015 15h22 đã test ok
            //Cỡ chữ 16 bên map là cỡ chữ 12 bên System.Drawing
            //
            Int16 Index_Resolution = 60;
            int indexMode = 0;
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height; AirSpeed_Index += (double)Height / 6)
            {
                //Ghi chu chi do phan giai
                //Ngày 16/12/2015 15h22 đã test ok
                indexMode++;
                Speed_ModeAutoRemove(((Int16)Air_Speed / 10 * 10 - 30 + Index_Resolution).ToString(),
                    16, BlushOfString1, PoinStart_X - 40, AirSpeed_Index - 10, 1, indexMode);
                Index_Resolution -= 10;
            }
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            //*************************************************************************
            //Ve Cho mau den de ghi Airspeed
            //Ngày 16/12/2015 15h52 đã test ok
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(Int16)(PoinStart_X + 15)
             */
            double SizeOfString = 24;
            double I16FullScale = 60, So_Khoang_Chia = 6;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            //1 ký tự rộng = SizeOfString * 3 / 4;
            double Config_Position = 12, i16StartStrAxisX;
            //double DoRongVungDen = Air_Speed.ToString().Length * (SizeOfString * 3 / 4);
            double DoRongVungDen = 26;
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = Air_Speed.ToString().Length * (SizeOfString * 0.6);
            if (Air_Speed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (Air_Speed.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (Air_Speed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Hình chữ nhật được căn lề phải
            i16StartStrAxisX = (PoinStart_X - 41);
            double StartXRect = (PoinStart_X - 10);
            FillRect_AutoRemove3(BlushRectangle4, StartXRect - DoRongVungDenRect, PoinStart_Y - Config_Position - 1 +
                (30 - Air_Speed % 10) * Height / 60, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            //Ngày 16/12/2015 15h52 đã test ok
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX + DoRongVungDen;
            y1 = PoinStart_Y - Config_Position +
                (30 - Air_Speed % 10) * Height / 60;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X + Width;
            y3 = (y1 + y2) / 2;
            Draw_TriAngle_Var(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)Air_Speed / 100
            //Ngày 16/12/2015 15h56 đã test ok
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)Air_Speed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)Air_Speed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)Air_Speed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            //drawFont = new Font("Arial", SizeOfString);
            //-2 là độ dời chữ vào trong thích hợp
            DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
                                PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);

            //Còn bên Notepad++
            //Khó quá bỏ qua

        }
        //*************************************************************************
        void Draw_TriAngle_Var(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            //Graphics G_TriAngle = this.CreateGraphics();
            //SolidBrush Blush_TriAngle = new SolidBrush(Color.Black);
            SolidColorBrush BlushOfTriAngle = new SolidColorBrush(Colors.Black);
            //Point[] points = { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
            PolygonAutoRemove(BlushOfTriAngle, x1, y1, x2, y2, x3, y3, 1);
        }
        //*****************************************************************************************************
        //*************************************************************************
        void Altitude_Draw_TriAngle(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            //Graphics G_TriAngle = this.CreateGraphics();
            //SolidBrush Blush_TriAngle = new SolidBrush(Color.Black);
            SolidColorBrush BlushOfTriAngle = new SolidColorBrush(Colors.Black);
            //Point[] points = { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3) };
            Altitude_PolygonAutoRemove(BlushOfTriAngle, x1, y1, x2, y2, x3, y3, 1);
        }
        //*****************************************************************************************************
        //Ngày 17/12/2015 Test tách chuỗi Data có thiết bị thực
        void SplitData_Test()
        {
            //string DataTest =
            //"-0011  0084 -0040 -0003  0012 -0007 -0143 -0016  0976  2687  0175  2041  0849 $GPVTG,110.33,T,,M,0.18,N,0.33,K,A * 34";

            //Tìm gia tốc bằng cách phát hiện chuỗi $GPGGA
            DataFromSensor Data = new DataFromSensor();
            //if (strDataFromSerialPort.IndexOf(Convert.ToChar(10).ToString() + Convert.ToChar(13)) != -1)
            {
                Data.Temp = "$GPVTG,30.40,T,,M,0.95,N,1.76,K,A ";
                //hàng từ 0 đến 9 là của Acc
                //Acc gồm 10 hàng
                //trong data cái đầu tiên chỉ có 9 hàng nên ta thêm nếu kí tự đầu tiên của chuỗi khác $ thì đó mới là Acc
                if ((Data.Temp[0] != '$') && Data.Temp.Length > 2) //Do có lúc chỉ có 1 hàng trắng
                {
                    Data.DataAcc = Data.Temp;
                    ////tbOutputText.Text += "ACC[" + (index_dataAcc).ToString() + "]: " + Data.DataAcc[index_dataAcc] + '\n';
                    index_dataAcc++;
                }

                //if (Data.Temp.IndexOf('$') >= 80)
                //tách lấy 1 giá trị gia tốc trong chuỗi 10 giá trị gia tốc thu được trong 100ms
                //Data.Acc = Data.Temp.Substring(Data.Temp.IndexOf('$') - 80, 80);
                ////tbOutputText.Text += "Temp: " + Data.Temp + '\n';
                //if(index_dataAcc <= 10 && (index_dataAcc > 0))

                //Tìm chuối bắt đầu với $GPGGA để tìm lat long, alt
                //Data.Temp = "$GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                if (Data.Temp.IndexOf("$GPG") != -1)
                {
                    //Chuỗi này chứa lat, long, alt
                    //$GPGGA,024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67
                    //Save Data to txt

                    //


                    //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                    //Data.Temp = "024004.900,1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                    ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                    //tách lấy giờ
                    Data.Time = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                    //Ngày 17/12/2015 17h36 ok
                    //tbOutputText.Text += "Time: " + Data.Time + '\n';
                    //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                    //Data.Temp = "1043.4006,N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                    ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                    //tách lấy vĩ độ mặc định ở vĩ độ North
                    Data.Latitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                    //tbOutputText.Text += "Latitude: " + Data.Latitude + '\n';
                    //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                    //Data.Temp = "N,10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                    //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                    //Data.Temp = "10641.3309,E,1,4,2.67,7.8,M,2.5,M,,*67 ";
                    //tách lấy kinh độ
                    Data.Longtitude = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                    //tbOutputText.Text += "Longtitude: " + Data.Longtitude + '\n';
                    //tách lấy độ cao so với mực nước biển
                    //tìm ký tự M đầu tiên trong chuỗi
                    //Temp =  $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9,M,2.5,M,,*6E
                    Data.Temp = Data.Temp.Substring(0, Data.Temp.IndexOf('M') - 1);
                    //Temp = $GPGGA,024005.000,1043.4007,N,10641.3308,E,1,4,2.67,7.9
                    //Độ cao là số sao dấu phẩy cuối cùng
                    Data.Altitude = Data.Temp.Substring(Data.Temp.LastIndexOf(',') + 1);
                    //tbOutputText.Text += "Altitude: " + Data.Altitude + '\n';

                }
                //Tìm chuối bắt đầu với $GPVTG để tìm angle
                if (Data.Temp.IndexOf("$GPV") != -1)
                {
                    //Chuỗi này chứa angle
                    //Reset bộ đếm số dòng của cảm biến IMU
                    index_dataAcc = 0;
                    //$GPVTG,350.40,T,,M,0.95,N,1.76,K,A

                    //cut bỏ Data đến dấu phẩy đầu tiên lấy sau dấu phẩy đầu tiên
                    Data.Temp = Data.Temp.Substring(Data.Temp.IndexOf(',') + 1, Data.Temp.Length - (Data.Temp.IndexOf(',') + 1));
                    //Data.Temp = "350.40,T,,M,0.95,N,1.76,K,A";
                    ////tbOutputText.Text += "DataSauCut: " + Data.Temp + '\n';
                    //tách lấy góc
                    Data.Angle = Data.Temp.Substring(0, Data.Temp.IndexOf(','));
                    //tbOutputText.Text += "Angle: " + Data.Angle + '\n';
                }
                ////tbOutputText.Text += "Acc: " + Data.Acc + '\n';
            }
        }
        //********************************************************************
        //Ngày 19/12/2015 Vẽ Compass
        //Đã test ok lúc 22h30
        //***************************************************
        //Ngay 02/ 10/ 2015
        //***************************************************
        /// <summary>
        /// Vẽ hiển thị của cảm biến Compass với tọa độ trung tâm là dComPass_mid_X, dComPass_mid_Y
        /// Bán kính là dComPass_R và nhận Angle_Flight là thông số đầu vào
        /// </summary>
        /// <param name="dComPass_mid_X"></param>
        /// <param name="dComPass_mid_Y"></param>
        /// <param name="dComPass_R"></param>
        /// <param name="Angle_Flight"></param>

        void ComPass_Setup(double Angle_Flight, double dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
        {

            double Angle_Rotate = Angle_Flight + 90;
            //Tao but ve
            //Graphics formGraphic = this.CreateGraphics();
            //Pen whitePen = new Pen(Color.White, 2);
            //Các bút vẽ cần thiết bên windows.UI
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfArrow = new SolidColorBrush(Colors.Green);
            //Ve background
            //không cần thiết
            double R_BackRound = dComPass_R - 12;
            //Compass_DrawArc(new Pen(Color.Red, 40), (dComPass_mid_X - R_BackRound), dComPass_mid_Y - R_BackRound,
            //    2 * R_BackRound, 2 * R_BackRound, 0, 360);
            //formGraphic.FillRectangle(Brushes.Brown, dComPass_mid_X - dComPass_R,
            //    dComPass_mid_Y - dComPass_R, 2 * dComPass_R, 2 * dComPass_R);
            //ngay 28/09/2015
            //formGraphic.DrawArc(whitePen, 200, 50, 200, 200, 210, 120);
            //Ve duong tron ben ngoai I(dComPass_mid_X, dComPass_mid_Y) ban kinh dComPass_R
            Compass_DrawArc(whitePen, (dComPass_mid_X - dComPass_R), dComPass_mid_Y - dComPass_R,
                2 * dComPass_R, 2 * dComPass_R, 0, 360);
            //Ve duong tron ben trong I(fmidXComPass, dComPass_mid_Y) ban kinh fRIntoComPass
            double dComPass_R_Into;
            dComPass_R_Into = dComPass_R - 20;

            //formGraphic.DrawArc(whitePen, (dComPass_mid_X - dComPass_R_Into), dComPass_mid_Y - dComPass_R_Into,
            // 2 * dComPass_R_Into, 2 * dComPass_R_Into, 0, 360);
            //Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            double fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into;
            /*
            fArc_X_15 = dComPass_R * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            fArc_Y_15 = - dComPass_R * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            fArc_Y_15_Into = - dComPass_R_Into * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            formGraphic.DrawLine(greenPen, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
             */
            //formGraphic.DrawPie(redPen, 100, 100, 100, 100, 0, 90);
            // Draw curve to screen.
            //formGraphic.DrawCurve(greenPen, curvePoints);

            //rmGraphic.DrawCurve(new Pen(Color.Red), 0, 0, 100);
            //rmGraphic.FillRectangle(Brushes.Green, 0, 0, 300, 300);
            //***********************************************************
            //***********************************************************
            /*
             * *Ngày 29/09/2015
             */

            /************************************************************/
            //Ve do chia cho Compass
            //double dComPass_mid_X = 300, dComPass_mid_Y = 250, dComPass_R = 100;
            //Ve duong tron ben ngoai I(dComPass_mid_X, dComPass_mid_Y) ban kinh dComPass_R
            Compass_DrawArc(whitePen, (dComPass_mid_X - dComPass_R), dComPass_mid_Y - dComPass_R,
                2 * dComPass_R, 2 * dComPass_R, 0, 360);
            //Ve duong tron ben trong I(fmidXComPass, dComPass_mid_Y) ban kinh fRIntoComPass
            //double dComPass_R_Into;
            dComPass_R_Into = dComPass_R - 10;
            //Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            //Ve tai 0, 30, 60, 90, 120, 150, 21, 24, 27, 30, 33 voi duong dai 15
            //fBalance_R = fBalance_R_Into + 10;
            //Ang_Rotate phu thuoc vao la ban

            /*************************************************/
            //Ve chuoi N, S, W, E 30, 60, 12, 15,...
            double draw_String_index;
            //Font drawFont = new Font("Arial", 12);
            double dSizeoftext = 16;
            double dOpacity = 1.0;
            int indexLine = 0;//chỉ số của đường 0 -->11

            //SolidBrush drawBrush = new SolidBrush(Color.White);
            for (double index = Angle_Rotate; index <= 330 + Angle_Rotate; index += 30)
            {
                fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;

                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                Compass_LineAutoRemove(indexLine, whitePen, 1);
                indexLine++;
                //Viet chu North tai vi tri goc ban dau Angle_Rotate

            }
            //*********************************************
            //viet tat ca cac so xuat hien trong Compass
            draw_String_index = Angle_Rotate;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(0, "N", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 30;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(1, "03", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 60;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(2, "06", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 90;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(3, "E", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 120;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(4, "12", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 150;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(5, "15", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 180;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(6, "S", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 210;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(7, "21", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 240;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(8, "24", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 270;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(9, "W", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 300;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(10, "30", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }
            draw_String_index = Angle_Rotate - 330;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupString_AutoRemove(11, "33", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            }


            //*********************************************************
            //Ve tai 10, 20, 40, 50, 70, 80, 100, ... voi duong dai 15
            //fBalance_R = fBalance_R_Into + 10;
            dComPass_R_Into = dComPass_R - 10;
            indexLine = 12;
            for (double index = Angle_Rotate; index <= 360 + Angle_Rotate; index += 10)
            {
                fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                Compass_LineAutoRemove(indexLine, whitePen, 1);
                indexLine++;
            }
            //Ve tai 5, 15, 25, 35, 45, 55, 65, ... voi duong dai 15
            //fBalance_R = fBalance_R_Into + 10;
            dComPass_R_Into = dComPass_R - 5;
            indexLine = 50;
            for (double index = Angle_Rotate; index <= 360 + Angle_Rotate; index += 5)
            {
                fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                Compass_LineAutoRemove(indexLine, whitePen, 1);
                indexLine++;
            }
            //Ngày 19/12/2015 Vẽ đường mũi tên chỉ hướng máy bay
            double dArrowY = dComPass_mid_Y - dComPass_R;
            double dArrowY1 = dComPass_mid_Y - dComPass_R / 3;
            double dArrowY2 = dComPass_mid_Y + dComPass_R / 3;
            double dArrowY3 = dComPass_mid_Y + dComPass_R;
            DrawLine(BlushOfArrow, 3, dComPass_mid_X, dArrowY1, dComPass_mid_X, dArrowY);
            PolygonHaveBrush(BlushOfArrow, dComPass_mid_X, dArrowY, dComPass_mid_X + 8, dArrowY + 12, dComPass_mid_X - 8, dArrowY + 12);
            PolygonHaveBrush(BlushOfArrow, dComPass_mid_X, dArrowY1, dComPass_mid_X + 4, dArrowY1 + 6, dComPass_mid_X - 4, dArrowY1 + 6);
            //đường thẳng ở phía dưới
            DrawLine(BlushOfArrow, 3, dComPass_mid_X, dArrowY2, dComPass_mid_X, dArrowY3);
            //Add Image
            Compass_AddFlightToCompass(dComPass_mid_X, dComPass_mid_Y);
            //bổ sang ngày 22/12/2015
            //*********************************************************
            //Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            //dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            //Vẽ mui tên màu đen 
            Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
                 dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            //Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            //Làm tròn thành số int
            //Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            //Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            Compass_Setup_Display_Angle(0, ((int)Angle_Flight).ToString() + '°', 22, new SolidColorBrush(Colors.Red),
                dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 36, 1, 25, 48);
            //ghi chữ Heading (degree) ở phía dưới
            DrawString("Heading (degrees)", 30, new SolidColorBrush(Colors.Purple), dComPass_mid_X - dComPass_R,
                dComPass_mid_Y + dComPass_R + 5, 1);
        }
        //*****************************************************************

        //Ngay 09/03/ 2015
        //***************************************************
        /// <summary>
        /// Vẽ hiển thị của cảm biến Compass với tọa độ trung tâm là dComPass_mid_X, dComPass_mid_Y
        /// Bán kính là dComPass_R và nhận Angle_Flight là thông số đầu vào
        /// </summary>
        /// <param name="dComPass_mid_X"></param>
        /// <param name="dComPass_mid_Y"></param>
        /// <param name="dComPass_R"></param>
        /// <param name="Angle_Flight"></param>

        void ComPass_Setup_Rotate_Out(double Angle_Flight, double dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
        {

            double Angle_Rotate = Angle_Flight + 90;

            //Ngày 19/12/2015 Vẽ đường mũi tên chỉ hướng máy bay
            SolidColorBrush BlushOfArrow = new SolidColorBrush(Colors.Green);
            double dArrowY = dComPass_mid_Y - dComPass_R;
            double dArrowY1 = dComPass_mid_Y - dComPass_R / 3;
            double dArrowY2 = dComPass_mid_Y + dComPass_R / 3;
            double dArrowY3 = dComPass_mid_Y + dComPass_R;
            DrawLine(BlushOfArrow, 5, dComPass_mid_X, dArrowY1, dComPass_mid_X, dArrowY);
            PolygonHaveBrush(BlushOfArrow, dComPass_mid_X, dArrowY, dComPass_mid_X + 8, dArrowY + 12, dComPass_mid_X - 8, dArrowY + 12);
            PolygonHaveBrush(BlushOfArrow, dComPass_mid_X, dArrowY1, dComPass_mid_X + 4, dArrowY1 + 6, dComPass_mid_X - 4, dArrowY1 + 6);
            //đường thẳng ở phía dưới
            DrawLine(BlushOfArrow, 5, dComPass_mid_X, dArrowY2, dComPass_mid_X, dArrowY3);
            //Add Image
            Compass_AddFlightToCompass(dComPass_mid_X, dComPass_mid_Y);
            //bổ sang ngày 22/12/2015
            //*********************************************************
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            //Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            //dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            //Vẽ mui tên màu đen 
            Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
                 dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            //Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            //Làm tròn thành số int
            //Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            //Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            Compass_Setup_Display_Angle(0, ((int)Angle_Flight).ToString() + '°', 22, new SolidColorBrush(Colors.Red),
                dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 36, 1, 25, 48);
            //ghi chữ Heading (degree) ở phía dưới
            DrawString("Heading", 30, new SolidColorBrush(Colors.Purple), dComPass_mid_X - dComPass_R + 55,
                dComPass_mid_Y + dComPass_R + 5, 1);
            DrawString("(degrees)", 30, new SolidColorBrush(Colors.Purple), dComPass_mid_X - dComPass_R + 50,
                dComPass_mid_Y + dComPass_R + 5 + 30, 1);
        }
        //*****************************************************************
        //Ngày 19/12/2015 Vẽ Compass
        //Đã test ok lúc 22h30
        //***************************************************
        //Ngay 02/ 10/ 2015
        //***************************************************
        /// <summary>
        /// Vẽ hiển thị của cảm biến Compass với tọa độ trung tâm là dComPass_mid_X, dComPass_mid_Y
        /// Bán kính là dComPass_R và nhận Angle_Flight là thông số đầu vào
        /// </summary>
        /// <param name="dComPass_mid_X"></param>
        /// <param name="dComPass_mid_Y"></param>
        /// <param name="dComPass_R"></param>
        /// <param name="Angle_Flight"></param>

        void ComPass_Draw_Compass(double Angle_Flight, double dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
        {

            //double Angle_Rotate = Angle_Flight + 90;
            ////Tao but ve
            ////Graphics formGraphic = this.CreateGraphics();
            ////Pen whitePen = new Pen(Color.White, 2);
            ////Các bút vẽ cần thiết bên windows.UI
            //SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            //SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            //SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            //SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            //SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            //SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            //SolidColorBrush BlushOfArrow = new SolidColorBrush(Colors.Green);
            ////Ve background
            ////không cần thiết
            //double R_BackRound = dComPass_R - 12;
            ////Compass_DrawArc(new Pen(Color.Red, 40), (dComPass_mid_X - R_BackRound), dComPass_mid_Y - R_BackRound,
            ////    2 * R_BackRound, 2 * R_BackRound, 0, 360);
            ////formGraphic.FillRectangle(Brushes.Brown, dComPass_mid_X - dComPass_R,
            ////    dComPass_mid_Y - dComPass_R, 2 * dComPass_R, 2 * dComPass_R);
            ////ngay 28/09/2015
            ////formGraphic.DrawArc(whitePen, 200, 50, 200, 200, 210, 120);
            ////Ve duong tron ben ngoai I(dComPass_mid_X, dComPass_mid_Y) ban kinh dComPass_R
            //Compass_DrawArc(whitePen, (dComPass_mid_X - dComPass_R), dComPass_mid_Y - dComPass_R,
            //    2 * dComPass_R, 2 * dComPass_R, 0, 360);
            ////Ve duong tron ben trong I(fmidXComPass, dComPass_mid_Y) ban kinh fRIntoComPass
            //double dComPass_R_Into;
            //dComPass_R_Into = dComPass_R - 20;

            ////formGraphic.DrawArc(whitePen, (dComPass_mid_X - dComPass_R_Into), dComPass_mid_Y - dComPass_R_Into,
            //// 2 * dComPass_R_Into, 2 * dComPass_R_Into, 0, 360);
            ////Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            //double fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into;
            ///*
            //fArc_X_15 = dComPass_R * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            //fArc_Y_15 = - dComPass_R * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            //fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            //fArc_Y_15_Into = - dComPass_R_Into * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            //formGraphic.DrawLine(greenPen, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
            // */
            ////formGraphic.DrawPie(redPen, 100, 100, 100, 100, 0, 90);
            //// Draw curve to screen.
            ////formGraphic.DrawCurve(greenPen, curvePoints);

            ////rmGraphic.DrawCurve(new Pen(Color.Red), 0, 0, 100);
            ////rmGraphic.FillRectangle(Brushes.Green, 0, 0, 300, 300);
            ////***********************************************************
            ////***********************************************************
            ///*
            // * *Ngày 29/09/2015
            // */

            ///************************************************************/
            ////Ve do chia cho Compass
            ////double dComPass_mid_X = 300, dComPass_mid_Y = 250, dComPass_R = 100;
            ////Ve duong tron ben ngoai I(dComPass_mid_X, dComPass_mid_Y) ban kinh dComPass_R
            //Compass_DrawArc(whitePen, (dComPass_mid_X - dComPass_R), dComPass_mid_Y - dComPass_R,
            //    2 * dComPass_R, 2 * dComPass_R, 0, 360);
            ////Ve duong tron ben trong I(fmidXComPass, dComPass_mid_Y) ban kinh fRIntoComPass
            ////double dComPass_R_Into;
            //dComPass_R_Into = dComPass_R - 10;
            ////Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            ////Ve tai 0, 30, 60, 90, 120, 150, 21, 24, 27, 30, 33 voi duong dai 15
            ////fBalance_R = fBalance_R_Into + 10;
            ////Ang_Rotate phu thuoc vao la ban

            ///*************************************************/
            ////Ve chuoi N, S, W, E 30, 60, 12, 15,...
            //double draw_String_index;
            ////Font drawFont = new Font("Arial", 12);
            //double dSizeoftext = 16;
            //double dOpacity = 1.0;
            //int indexLine = 0;//chỉ số của đường 0 -->11
            ////SolidBrush drawBrush = new SolidBrush(Color.White);
            //for (double index = Angle_Rotate; index <= 330 + Angle_Rotate; index += 30)
            //{
            //    fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
            //    fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;

            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
            //    Compass_LineAutoRemove(indexLine, whitePen, 1, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
            //    indexLine++;
            //    //Viet chu North tai vi tri goc ban dau Angle_Rotate

            //}
            ////*********************************************
            ////viet tat ca cac so xuat hien trong Compass
            //draw_String_index = Angle_Rotate;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(0, "N", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 30;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(1, "03", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 60;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(2, "06", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 90;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(3, "E", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 120;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(4, "12", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 150;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(5, "15", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 180;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(6, "S", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 210;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(7, "21", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 240;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(8, "24", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 270;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(9, "W", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 300;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(10, "30", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            //draw_String_index = Angle_Rotate - 330;
            //{
            //    dComPass_R_Into = dComPass_R - 22;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
            //    Compass_SetupString_AutoRemove(11, "33", dSizeoftext, whitePen, fArc_X_15_Into - 10, fArc_Y_15_Into - 9, dOpacity);
            //}
            ////*********************************************************
            ////*********************************************************
            ////Ve tai 10, 20, 40, 50, 70, 80, 100, ... voi duong dai 15
            ////fBalance_R = fBalance_R_Into + 10;
            //dComPass_R_Into = dComPass_R - 10;
            //indexLine = 12;
            //for (double index = Angle_Rotate; index <= 360 + Angle_Rotate; index += 10)
            //{
            //    fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
            //    fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
            //    Compass_LineAutoRemove(indexLine, whitePen, 1, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
            //    indexLine++;
            //}
            ////Ve tai 5, 15, 25, 35, 45, 55, 65, ... voi duong dai 15
            ////fBalance_R = fBalance_R_Into + 10;
            //dComPass_R_Into = dComPass_R - 5;
            //indexLine = 50;
            //for (double index = Angle_Rotate; index <= 360 + Angle_Rotate; index += 5)
            //{
            //    fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
            //    fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
            //    fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
            //    fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
            //    Compass_LineAutoRemove(indexLine, whitePen, 1, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
            //    indexLine++;
            //}
            ////bổ sang ngày 22/12/2015
            ////*********************************************************
            ////Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            ////dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            //Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            ////Vẽ mui tên màu đen 
            //Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
            //     dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            ////Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            ////Làm tròn thành số int
            ////Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            ////Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            //Compass_Setup_Display_Angle(0, ((int)Angle_Flight).ToString() + '°', 22, new SolidColorBrush(Colors.Red),
            //    dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 36, 1, 25, 48);

        }
        //*****************************************************************

        void ComPass_Draw_Compass_optimize(double Angle_Flight, double dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
        {

            double Angle_Rotate = Angle_Flight + 90;
            //Tao but ve
            //Graphics formGraphic = this.CreateGraphics();
            //Pen whitePen = new Pen(Color.White, 2);
            //Các bút vẽ cần thiết bên windows.UI
            //SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            //SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            //SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            //SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            //SolidColorBrush BlushOfArrow = new SolidColorBrush(Colors.Green);
            //Ve background
            //không cần thiết
            //double R_BackRound = dComPass_R - 12;
            //Compass_DrawArc(new Pen(Color.Red, 40), (dComPass_mid_X - R_BackRound), dComPass_mid_Y - R_BackRound,
            //    2 * R_BackRound, 2 * R_BackRound, 0, 360);
            //formGraphic.FillRectangle(Brushes.Brown, dComPass_mid_X - dComPass_R,
            //    dComPass_mid_Y - dComPass_R, 2 * dComPass_R, 2 * dComPass_R);
            //ngay 28/09/2015
            //formGraphic.DrawArc(whitePen, 200, 50, 200, 200, 210, 120);
            //Ve duong tron ben ngoai I(dComPass_mid_X, dComPass_mid_Y) ban kinh dComPass_R

            //Compass_DrawArc(whitePen, (dComPass_mid_X - dComPass_R), dComPass_mid_Y - dComPass_R,
            //    2 * dComPass_R, 2 * dComPass_R, 0, 360);

            //Ve duong tron ben trong I(fmidXComPass, dComPass_mid_Y) ban kinh fRIntoComPass
            double dComPass_R_Into;
            //dComPass_R_Into = dComPass_R - 20;

            //formGraphic.DrawArc(whitePen, (dComPass_mid_X - dComPass_R_Into), dComPass_mid_Y - dComPass_R_Into,
            // 2 * dComPass_R_Into, 2 * dComPass_R_Into, 0, 360);
            //Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            double fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into;
            /*
            fArc_X_15 = dComPass_R * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            fArc_Y_15 = - dComPass_R * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(pi / 18) + dComPass_mid_X;
            fArc_Y_15_Into = - dComPass_R_Into * (double)Math.Sin(pi / 18) + dComPass_mid_Y;
            formGraphic.DrawLine(greenPen, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
             */
            //formGraphic.DrawPie(redPen, 100, 100, 100, 100, 0, 90);
            // Draw curve to screen.
            //formGraphic.DrawCurve(greenPen, curvePoints);

            //rmGraphic.DrawCurve(new Pen(Color.Red), 0, 0, 100);
            //rmGraphic.FillRectangle(Brushes.Green, 0, 0, 300, 300);
            //***********************************************************
            //***********************************************************
            /*
             * *Ngày 29/09/2015
             */

            /************************************************************/
            //Ve do chia cho Compass
            //double dComPass_mid_X = 300, dComPass_mid_Y = 250, dComPass_R = 100;
            //Ve duong tron ben ngoai I(dComPass_mid_X, dComPass_mid_Y) ban kinh dComPass_R

            //Compass_DrawArc(whitePen, (dComPass_mid_X - dComPass_R), dComPass_mid_Y - dComPass_R,
            //    2 * dComPass_R, 2 * dComPass_R, 0, 360);

            //Ve duong tron ben trong I(fmidXComPass, dComPass_mid_Y) ban kinh fRIntoComPass
            //double dComPass_R_Into;
            dComPass_R_Into = dComPass_R - 10;
            //Ve duong thang do chia goc 15 do co 2 diem thuoc 2 duong tron
            //Ve tai 0, 30, 60, 90, 120, 150, 21, 24, 27, 30, 33 voi duong dai 15
            //fBalance_R = fBalance_R_Into + 10;
            //Ang_Rotate phu thuoc vao la ban

            /*************************************************/
            //Ve chuoi N, S, W, E 30, 60, 12, 15,...
            double draw_String_index;
            //Font drawFont = new Font("Arial", 12);
            //double dSizeoftext = 16;
            //double dOpacity = 1.0;
            int indexLine = 0;//chỉ số của đường 0 -->11
            //SolidBrush drawBrush = new SolidBrush(Color.White);
            for (double index = Angle_Rotate; index <= 330 + Angle_Rotate; index += 30)
            {
                fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;

                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                Compass_LineAutoRemove_run(indexLine, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
                indexLine++;
                //Viet chu North tai vi tri goc ban dau Angle_Rotate

            }
            //*********************************************
            //viet tat ca cac so xuat hien trong Compass
            draw_String_index = Angle_Rotate;
            {
                dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(0, "N", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 30;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(1, "03", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 60;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(2, "06", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 90;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(3, "E", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 120;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(4, "12", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 150;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(5, "15", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 180;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(6, "S", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 210;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(7, "21", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 240;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(8, "24", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 270;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(9, "W", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 300;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(10, "30", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            draw_String_index = Angle_Rotate - 330;
            {
                //dComPass_R_Into = dComPass_R - 22;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * draw_String_index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * draw_String_index / 180) + dComPass_mid_Y;
                Compass_SetupStr_AutoRem_run(11, "33", fArc_X_15_Into - 10, fArc_Y_15_Into - 9);
            }
            //*********************************************************
            //*********************************************************
            //Ve tai 10, 20, 40, 50, 70, 80, 100, ... voi duong dai 15
            //fBalance_R = fBalance_R_Into + 10;
            dComPass_R_Into = dComPass_R - 10;
            indexLine = 12;
            for (double index = Angle_Rotate; index <= 360 + Angle_Rotate; index += 10)
            {
                fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                Compass_LineAutoRemove_run(indexLine, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
                indexLine++;
            }
            //Ve tai 5, 15, 25, 35, 45, 55, 65, ... voi duong dai 15
            //fBalance_R = fBalance_R_Into + 10;
            dComPass_R_Into = dComPass_R - 5;
            indexLine = 50;
            for (double index = Angle_Rotate; index <= 360 + Angle_Rotate; index += 5)
            {
                fArc_X_15 = dComPass_R * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15 = -dComPass_R * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                fArc_X_15_Into = dComPass_R_Into * (double)Math.Cos(Math.PI * index / 180) + dComPass_mid_X;
                fArc_Y_15_Into = -dComPass_R_Into * (double)Math.Sin(Math.PI * index / 180) + dComPass_mid_Y;
                Compass_LineAutoRemove_run(indexLine, fArc_X_15, fArc_Y_15, fArc_X_15_Into, fArc_Y_15_Into);
                indexLine++;
            }
            //bổ sang ngày 22/12/2015
            //*********************************************************
            //Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            //dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            //Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            //Vẽ mui tên màu đen 
            //Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
            //     dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            //Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            //Làm tròn thành số int
            //Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            //Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            //đổi số âm để dễ hiển thị
            if (Angle_Flight < 0) Angle_Flight += 360;
            Compass_Display_Angle_run(0, ((int)Angle_Flight).ToString() + '°', dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 36);

        }
        //*****************************************************************
        //*****************************************************************

        void ComPass_Draw_Com_opt_RotOut(double Angle_Flight, double dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
        {

            double Angle_Rotate = Angle_Flight + 90;


            //bổ sang ngày 22/12/2015
            //*********************************************************
            //Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            //dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            //Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            //Vẽ mui tên màu đen 
            //Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
            //     dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            //Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            //Làm tròn thành số int
            //Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            //Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            //đổi số âm để dễ hiển thị
            if (Angle_Flight < 0) Angle_Flight += 360;
            Compass_Display_Angle_run(0, ((int)Angle_Flight).ToString() + '°', dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 36);

        }
        //*****************************************************************
        //*******************************************************************
        //Bước đột phá, tạo mảng hình chữ nhật auto remove biến này là biến toàn cục
        Rectangle[] Compass_Ret_AutoRemove = new Rectangle[2];

        //Set up for rectangle
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Compass_Rect_Setup(int index, SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Compass_Ret_AutoRemove[index]);
            Compass_Ret_AutoRemove[index] = new Rectangle();
            Compass_Ret_AutoRemove[index].Fill = Blush;
            Compass_Ret_AutoRemove[index].Height = height;
            Compass_Ret_AutoRemove[index].Width = width;
            Compass_Ret_AutoRemove[index].Opacity = Opacity;
            //Xac định tọa độ
            Compass_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Compass_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Compass_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Compass_Ret_AutoRemove[index]);

        }
        //********************************************************************************

        /// <summary>
        /// Khi tác động vào hình chữ nhật thì remove cái cũ và add vị trí mới
        /// màu sắc và độ đục đã được set up rồi
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Compass_Rect_Change_AutoRemove(int index, double StartX, double StartY, double width, double height)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Compass_Ret_AutoRemove[index]);
            Compass_Ret_AutoRemove[index].Height = height;
            Compass_Ret_AutoRemove[index].Width = width;
            //Xac định tọa độ
            Compass_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Compass_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Compass_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Compass_Ret_AutoRemove[index]);

        }
        //********************************************************************************
        Polygon Compass_PolygonAutoRemove = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Compass_Poly_AutoRemove(SolidColorBrush BlushOfTriAngle, double x1, double y1, double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));
            //Polygon Compass_PolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(Compass_PolygonAutoRemove);
            Compass_PolygonAutoRemove.Points = myPointCollection;
            Compass_PolygonAutoRemove.Fill = BlushOfTriAngle;
            Compass_PolygonAutoRemove.Width = (xmax - xmin);
            Compass_PolygonAutoRemove.Height = (ymax - ymin);
            Compass_PolygonAutoRemove.Stretch = Stretch.Fill;
            Compass_PolygonAutoRemove.Stroke = BlushOfTriAngle;
            Compass_PolygonAutoRemove.Opacity = Opacity;
            Compass_PolygonAutoRemove.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + Compass_PolygonAutoRemove.Width - (200 - 2 * xmin), -600 + Compass_PolygonAutoRemove.Height, 0, 0);
            //Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + Compass_PolygonAutoRemove.Height + 4 * slider.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //Compass_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(Compass_PolygonAutoRemove);

        }
        //*********************************************************************************************
        //**************Ngày 08/12/2015************************************
        Image img_FliCom = new Image();//biến này để add flight
        Image img_FliCom_Out = new Image();//biến này để quay phía ngoài
        //Hiện ảnh ở tọa độ trung tâm x, y
        //Ngày 19/12/2015 test ok
        /// <summary>
        /// Chọn background cho các cảm biến
        /// Lấy một hình vẽ bất kỳ làm background
        /// chỉnh lại thành 2 màn hình, khi đó bản đồ bị thu lại
        /// </summary>
        public void Compass_AddFlightToCompass(double CenterX, double CenterY)
        {
            ////Image img_FliCom = new Image();
            //BackgroundDisplay.Children.Remove(img_FliCom);
            ////Edit size of image
            //img_FliCom.Height = 60;
            //img_FliCom.Width = 60;

            ////img_FliCom.RenderTransform
            //img_FliCom.Opacity = 1;


            ////img_FliCom.Transitions.
            //img_FliCom.Source = new BitmapImage(new Uri("ms-appx:///Assets/ImCompass.png"));
            ////Xoay ảnh
            ////kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            ////Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            ////khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            ////Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            ////dời ảnh lên trên 1 nửa chiều dài,
            ////dời ảnh sang trái 1 nửa chiều rộng
            //img_FliCom.RenderTransform = new RotateTransform()
            //{

            //    //Angle = 0,
            //    //CenterX = 40,//là la img_FliCom_FliCom.Width/2
            //    //CenterX = 62, //The prop name maybe mistyped 
            //    //CenterY = 40 //la img_FliCom_FliCom.Height
            //};
            ////mặc định ảnh có chiều dài và chiều rộng là vô cùng
            ////bitmapImage.PixelHeight
            ////img_FliCom.sca
            //img_FliCom.Stretch = Stretch.Uniform;
            //img_FliCom.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            //img_FliCom.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            //img_FliCom.Opacity = 1;
            //img_FliCom.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + CenterX * 2, -798  +  dConvertToTabletY + CenterY * 2, 0, 0);
            //BackgroundDisplay.Children.Add(img_FliCom);
            //***************************************************************************************
            //9/03/2016 Add thêm ảnh mới
            //Image img_FliCom_Out = new Image();
            BackgroundDisplay.Children.Remove(img_FliCom_Out);
            //Edit size of image
            img_FliCom_Out.Height = 250;
            img_FliCom_Out.Width = 250;

            //img_FliCom_Out.RenderTransform
            img_FliCom_Out.Opacity = 1;


            //img_FliCom_Out.Transitions.
            img_FliCom_Out.Source = new BitmapImage(new Uri("ms-appx:///Assets/Compass_Rose_English_North.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            img_FliCom_Out.RenderTransform = new RotateTransform()
            {

                //Angle = 0,
                //CenterX = 40,//là la img_FliCom_Out_FliCom.Width/2
                //CenterX = 62, //The prop name maybe mistyped 
                //CenterY = 40 //la img_FliCom_Out_FliCom.Height
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img_FliCom_Out.sca
            img_FliCom_Out.Stretch = Stretch.Uniform;
            img_FliCom_Out.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img_FliCom_Out.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            img_FliCom_Out.Opacity = 0.8;
            img_FliCom_Out.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + CenterX * 2, -798 + dConvertToTabletY + CenterY * 2, 0, 0);
            BackgroundDisplay.Children.Add(img_FliCom_Out);

        }
        //Ngày 04/03/2016
        //
        //**************Ngày 08/12/2015************************************
        //Hiện ảnh ở tọa độ trung tâm x, y
        //Ngày 19/12/2015 test ok
        /// <summary>
        ///Ngày 04/03/2016 xoay máy bay và thay đổi góc yaw hiển thị
        ///Remove cái cũ sau đó add cái mới
        /// </summary>
        public void Comp_RotateAndAddValue(double angle_Yaw)
        {

            BackgroundDisplay.Children.Remove(img_FliCom);
            //center width/2, height
            img_FliCom.RenderTransform = new RotateTransform()
            {

                Angle = angle_Yaw,
                CenterX = 40,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 40
            };

            //img_FliCom.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + CenterX * 2, -798  +  dConvertToTabletY + CenterY * 2, 0, 0);
            BackgroundDisplay.Children.Add(img_FliCom);

            ///Add value*************************************************
            ///            //bổ sang ngày 22/12/2015
            //*********************************************************
            //Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            //dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            //Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            //Vẽ mui tên màu đen 
            //Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
            //    dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            //Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            //Làm tròn thành số int
            //Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            //Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            //****************************************
            BackgroundDisplay.Children.Remove(Tb_Compass_Display_Angle[0]);

            if (angle_Yaw < 0) angle_Yaw += 360;//chỉnh góc angle_Yaw > 0 để dễ hiển thị
            Tb_Compass_Display_Angle[0].Text = (Math.Round(angle_Yaw, 0)).ToString() + '°';

            //Tb_Compass_Display_Angle[index].FontStyle = "Arial";
            //Tb_Compass_Display_Angle[index].FontStretch
            //color text có độ đục
            //Tb_Compass_Display_Angle[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //position of text left, top, right, bottom

            BackgroundDisplay.Children.Add(Tb_Compass_Display_Angle[0]);
            //BackgroundDisplay.Visibility;

        }
        //************************************************************
        //Ngày 09/03/2016
        //
        //**************Ngày 08/12/2015************************************
        //Hiện ảnh ở tọa độ trung tâm x, y
        //Ngày 19/12/2015 test ok
        /// <summary>
        ///Ngày 04/03/2016 xoay máy bay và thay đổi góc yaw hiển thị
        ///Remove cái cũ sau đó add cái mới
        /// </summary>
        public void Comp_Rotate_OutAndAddValue(double angle_Yaw)
        {

            BackgroundDisplay.Children.Remove(img_FliCom_Out);
            //center width/2, height/2
            img_FliCom_Out.RenderTransform = new RotateTransform()
            {

                Angle = 360 - angle_Yaw,
                CenterX = 125,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 125
            };

            //img_FliCom.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + CenterX * 2, -798  +  dConvertToTabletY + CenterY * 2, 0, 0);
            BackgroundDisplay.Children.Add(img_FliCom_Out);

            ///Add value*************************************************
            ///            //bổ sang ngày 22/12/2015
            //*********************************************************
            //Vẽ hình chữ nhật ghi góc, vẽ mũi tên và ghi giá trí góc
            //dComPass_mid_X, double dComPass_mid_Y, double dComPass_R)
            //Compass_Rect_Setup(0, BlushRectangle4, dComPass_mid_X - 24, dComPass_mid_Y - dComPass_R - 35, 48, 25, 1);
            //Vẽ mui tên màu đen 
            //Compass_Poly_AutoRemove(BlushRectangle4, dComPass_mid_X - 5, dComPass_mid_Y - dComPass_R - 10,
            //    dComPass_mid_X + 5, dComPass_mid_Y - dComPass_R - 10, dComPass_mid_X, dComPass_mid_Y - dComPass_R, 1);
            //Nếu trong Angle_Flight có dấu . thì chỉ lấy 1 ký tự sau dấu chấm thập phân
            //Làm tròn thành số int
            //Vẽ String có căn lề trung tâm và format bên trong hình chữ nhật
            //Compass_SetupString_AutoRemove(12, Angle_Flight.ToString(), 22, whitePen, dComPass_mid_X - 20, dComPass_mid_Y - dComPass_R - 35, 1);
            //****************************************
            BackgroundDisplay.Children.Remove(Tb_Compass_Display_Angle[0]);

            if (angle_Yaw < 0) angle_Yaw += 360;//chỉnh góc angle_Yaw > 0 để dễ hiển thị
            Tb_Compass_Display_Angle[0].Text = (Math.Round(angle_Yaw, 0)).ToString() + '°';

            //Tb_Compass_Display_Angle[index].FontStyle = "Arial";
            //Tb_Compass_Display_Angle[index].FontStretch
            //color text có độ đục
            //Tb_Compass_Display_Angle[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            //position of text left, top, right, bottom

            BackgroundDisplay.Children.Add(Tb_Compass_Display_Angle[0]);
            //BackgroundDisplay.Visibility;

        }


        //******************************************************************************
        //Ngày 19/12/2015 Vẽ Altitude
        /// <summary>
        /// Vẽ Altitude của cảm biến
        /// Tọa độ đầu của hình chữ nhật là PoinStart_X, PoinStart_Y
        /// </summary>
        /// <param name="fAltitude"></param>
        void Altitude_Draw_Alt(double dAltitude, double PoinStart_X, double PoinStart_Y)
        {
            //Graphics formGraphic = this.CreateGraphics();

            //Create pens.
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            //Các bút vẽ cần thiết bên windows.UI
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            SolidColorBrush greenPen = new SolidColorBrush(Colors.Green);
            // Draw lines between original podoubles to screen.
            //formGraphic.DrawLines(redPen, curvePodoubles);
            double Width = 8, Height = 250;
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //Diem bat dau ben trai la PoinStart_X
            //Draw BackGround
            //Edit bên map ngày 19/12/2015

            if (dAltitude.ToString().Length > 5)
            {
                Rect_Change_AutoRemove(1, PoinStart_X, PoinStart_Y - 8,
                    80 + (dAltitude.ToString().Length - 5) * 14 + Width, Blue_BackGround - (PoinStart_Y - 8));
                Rect_Change_AutoRemove(2, PoinStart_X, Blue_BackGround,
                    80 + (dAltitude.ToString().Length - 5) * 14 + Width, (PoinStart_Y + Height + 5) - Blue_BackGround);
            }
            else
            {
                Rect_Change_AutoRemove(1, PoinStart_X, PoinStart_Y - 8, 80 + Width,
                    Blue_BackGround - (PoinStart_Y - 8));
                Rect_Change_AutoRemove(2, PoinStart_X, Blue_BackGround, 80 + Width,
                   (PoinStart_Y + Height + 5) - Blue_BackGround);
            }

            //Ve duong vien mau trang

            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePodoubles);
            DrawLine(whitePen, 1, PoinStart_X, PoinStart_Y - 10, PoinStart_X, PoinStart_Y + Height + 8);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Font drawFont = new Font("Arial", 12);
            //SolidBrush drawBrush = new SolidBrush(Color.White);
            //Edit bên map
            //double dSizeOfText = 16;
            //Khoang cach giua diem cao nhat va thap nhat va chia lam bao nhieu khoang
            double Index_Resolution = 600, I16FullScale = 600, So_Khoang_Chia = 6;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            int index_str = 1;//là chỉ số của string trong mảng Tb_Alt 
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / So_Khoang_Chia)
            {

                //Ghi chu chi do phan giai
                /*Phep toan lam tron den do phan giai can hien thi, hang tram iDoPhanGiai = I16FullScale / So_Khoang_Chia = 100;
                *dAltitude / iDoPhanGiai * iDoPhanGiai :so nay o  vi tri trung tam
                */
                /*So o vi tri thap nhat
                 * dAltitude / iDoPhanGiai * iDoPhanGiai - I16FullScale / 2
                 */
                /*
                 * Vi tri bat dau cua chu la sau vat ke vi tri tai: (PoinStart_X + 15, AirSpeed_Index - 10)
                 * So -10 de canh chinh chu cho phu hop
                 */
                //Viet tu tren xuong duoi chi so y tang dan: AirSpeed_Index += (double)Height / So_Khoang_Chia
                //nhung chi so do cao giam dan: Index_Resolution -= iDoPhanGiai;
                Alt_ChangeString_AutoRemove(index_str, ((Int32)dAltitude / (Int32)iDoPhanGiai * (Int32)iDoPhanGiai - I16FullScale / 2 +
                    Index_Resolution).ToString(), PoinStart_X + 15, AirSpeed_Index - 10);
                Index_Resolution -= iDoPhanGiai;
                index_str++;
            }

            //Ve Cho mau den de ghi Airspeed hang nghin va hang tram
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(PoinStart_X + 15)
             */

            //Khúc trên ok
            //Còn bên notepad
            double Config_Position = 12, i16StartStrAxisX;
            i16StartStrAxisX = (PoinStart_X + 15);

            double SizeOfString = 24;
            //Thay đổi độ rộng vùng đen
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = dAltitude.ToString().Length * (SizeOfString * 0.6);
            if (dAltitude.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (dAltitude.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (dAltitude.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Vẽ hình chữ nhật mà đen để hiện số tự động remove khi hình mới xuất hiện
            //Hình chữ nhật màu đen của Alt có chỉ số là 0
            Rect_Change_AutoRemove(0, i16StartStrAxisX, PoinStart_Y - Config_Position - 1 +
                    (300 - dAltitude % 100) * Height / 600, DoRongVungDenRect, Config_Position * 2);

            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX;
            y1 = PoinStart_Y - Config_Position +
                (300 - dAltitude % 100) * Height / 600;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X;
            y3 = (y1 + y2) / 2;
            Altitude_Draw_TriAngle(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)fAltitude / 100
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)fAltitude % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)fAltitude / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)fAltitude % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)fAltitude % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //drawFont = new Font("Arial", SizeOfString);
            //DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
            //PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);
            //Chữ trong màu đen có chỉ số là 0
            Alt_ChangeString_AutoRemove(0, dAltitude.ToString(), PoinStart_X + 15,
                                PoinStart_Y - 15 + (300 - dAltitude % 100) * Height / I16FullScale);

        }
        //**************************************************************************
        //******************************************************************************
        //Ngày 19/12/2015 Vẽ Altitude
        /// <summary>
        /// Set up vị trí Altitude của cảm biến
        /// Tọa độ đầu của hình chữ nhật là PoinStart_X, PoinStart_Y
        /// </summary>
        /// <param name="fAltitude"></param>
        void Altitude_Setup(double dAltitude, double PoinStart_X, double PoinStart_Y)
        {
            //Graphics formGraphic = this.CreateGraphics();

            //Create pens.
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            //Các bút vẽ cần thiết bên windows.UI
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            SolidColorBrush greenPen = new SolidColorBrush(Colors.Green);
            // Draw lines between original podoubles to screen.
            //formGraphic.DrawLines(redPen, curvePodoubles);
            double Width = 8, Height = 250;
            //Viết chữ Altitude
            DrawString("Altitude (m)", 30, new SolidColorBrush(Colors.Green), PoinStart_X - 30, PoinStart_Y + Height + 5, 0.8);
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //Diem bat dau ben trai la PoinStart_X
            //Draw BackGround
            //Edit bên map ngày 19/12/2015

            if (dAltitude.ToString().Length > 5)
            {
                Rect_Setup_AutoRemove(1, BlushRectangle1, PoinStart_X, PoinStart_Y - 8,
                    80 + (dAltitude.ToString().Length - 5) * 14 + Width, Blue_BackGround - (PoinStart_Y - 8), 0.3);
                Rect_Setup_AutoRemove(2, BlushRectangle2, PoinStart_X, Blue_BackGround,
                    80 + (dAltitude.ToString().Length - 5) * 14 + Width, (PoinStart_Y + Height + 5) - Blue_BackGround, 0.3);
            }
            else
            {
                Rect_Setup_AutoRemove(1, BlushRectangle1, PoinStart_X, PoinStart_Y - 8, 80 + Width,
                    Blue_BackGround - (PoinStart_Y - 8), 0.3);
                Rect_Setup_AutoRemove(2, BlushRectangle2, PoinStart_X, Blue_BackGround, 80 + Width,
                   (PoinStart_Y + Height + 5) - Blue_BackGround, 0.3);
            }

            //Ve duong vien mau trang

            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePodoubles);
            DrawLine(whitePen, 1, PoinStart_X, PoinStart_Y - 10, PoinStart_X, PoinStart_Y + Height + 8);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Font drawFont = new Font("Arial", 12);
            //SolidBrush drawBrush = new SolidBrush(Color.White);
            //Edit bên map
            double dSizeOfText = 16;
            //Khoang cach giua diem cao nhat va thap nhat va chia lam bao nhieu khoang
            double Index_Resolution = 600, I16FullScale = 600, So_Khoang_Chia = 6;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            int index_str = 1;//là chỉ số của string trong mảng Tb_Alt 
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / So_Khoang_Chia)
            {
                //-1 de duong ke ngang co do rong phu hop tai vi tri muon ke.
                DrawLine(whitePen, 3, PoinStart_X, AirSpeed_Index - 1, PoinStart_X + 15, AirSpeed_Index - 1);
                //Ghi chu chi do phan giai
                /*Phep toan lam tron den do phan giai can hien thi, hang tram iDoPhanGiai = I16FullScale / So_Khoang_Chia = 100;
                *dAltitude / iDoPhanGiai * iDoPhanGiai :so nay o  vi tri trung tam
                */
                /*So o vi tri thap nhat
                 * dAltitude / iDoPhanGiai * iDoPhanGiai - I16FullScale / 2
                 */
                /*
                 * Vi tri bat dau cua chu la sau vat ke vi tri tai: (PoinStart_X + 15, AirSpeed_Index - 10)
                 * So -10 de canh chinh chu cho phu hop
                 */
                //Viet tu tren xuong duoi chi so y tang dan: AirSpeed_Index += (double)Height / So_Khoang_Chia
                //nhung chi so do cao giam dan: Index_Resolution -= iDoPhanGiai;
                Alt_SetupString_AutoRemove(index_str, ((Int32)dAltitude / (Int32)iDoPhanGiai * (Int32)iDoPhanGiai - I16FullScale / 2 +
                    Index_Resolution).ToString(), dSizeOfText, whitePen, PoinStart_X + 15, AirSpeed_Index - 10, 1);
                Index_Resolution -= iDoPhanGiai;
                index_str++;
            }
            //Ke nhung doan ngan hon va khong ghi so
            for (double AirSpeed_Index = PoinStart_Y + Height / 12; AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / So_Khoang_Chia)
            {
                DrawLine(whitePen, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X, AirSpeed_Index - 1);
            }
            //Ve Cho mau den de ghi Airspeed hang nghin va hang tram
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(PoinStart_X + 15)
             */

            //Khúc trên ok
            //Còn bên notepad
            double Config_Position = 12, i16StartStrAxisX;
            i16StartStrAxisX = (PoinStart_X + 15);

            double SizeOfString = 24;
            //Thay đổi độ rộng vùng đen
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = dAltitude.ToString().Length * (SizeOfString * 0.6);
            if (dAltitude.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (dAltitude.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (dAltitude.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Vẽ hình chữ nhật mà đen để hiện số tự động remove khi hình mới xuất hiện
            //Hình chữ nhật màu đen của Alt có chỉ số là 0
            Rect_Setup_AutoRemove(0, BlushRectangle4, i16StartStrAxisX, PoinStart_Y - Config_Position - 1 +
                    (300 - dAltitude % 100) * Height / 600, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX;
            y1 = PoinStart_Y - Config_Position +
                (300 - dAltitude % 100) * Height / 600;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X;
            y3 = (y1 + y2) / 2;
            Altitude_Draw_TriAngle(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)fAltitude / 100
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)fAltitude % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)fAltitude / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)fAltitude % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)fAltitude % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //drawFont = new Font("Arial", SizeOfString);
            //DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
            //PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);
            //Chữ trong màu đen có chỉ số là 0
            Alt_SetupString_AutoRemove(0, dAltitude.ToString(), SizeOfString, whitePen, PoinStart_X + 15,
                                PoinStart_Y - 15 + (300 - dAltitude % 100) * Height / I16FullScale, 1);

        }
        //**************************************************************************
        //Ngày 20/12/2015
        /// <summary>
        /// Vẽ Background Bằng cách cut map and Zoom map
        /// Tạo 2 map, map 2 là zoom của map chính tại vị trí của máy bay.
        /// CenterX là điểm chia 2 Map
        /// ZoomLevel là Zoom leval of Background
        /// </summary>
        /// <param name="CenterX"></param>
        /// <param name="CenterY"></param>
        /// <param name="ZoomLevel"></param>
        void Background_TestDrawBackground(double CenterX, double CenterY, double ZoomLevel)
        {
            //MapControl MapBackground = new MapControl();
            //MapBackground.Loaded += MyMap_Loaded_Background;
            MapBackground.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle San Bay Tan Son Nhat: dLatDentination, dLonDentination
                   Latitude = dLatDentination,
                   Longitude = dLonDentination
               });
            MapBackground.ZoomLevel = 17;
            MapBackground.Style = MapStyle.Road;
            myMap.Width = 1363 - CenterX;
            myMap.Margin = new Windows.UI.Xaml.Thickness(CenterX, 0, 00, 00);
            MapBackground.Width = CenterX;



            //MapBackground.di
            //MapBackground.di
            //MapBackground.Unloaded = false;
            //MapBackground.Focus(Windows.UI.Xaml.FocusState.Unfocused);

        }
        //**************************************************************************
        //Ngày 20/12/2015 Set up góc Pitch
        /// <summary>
        /// Vẽ góc Pitch, Roll của máy bay
        /// Với StartX, StartY là điểm trung tâm
        /// h1: khoảng cách giữa 2 đường
        /// </summary>
        /// <param name="Pitch"></param>
        /// <param name="StartX"></param>
        /// <param name=""></param>
        void PitchAngle_Setup(double Pitch, double Roll, double StartX, double StartY, double h1)
        {


            //*************************************************************************
            //Cách 2, giữ nguyên đường màu vàng mỗi lần góc Pitch thay đổi thì toàn bộ các đường song song 
            //chạy xuống hoặc chạy lên
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            double R1 = 40, R2;//R1, R2 là độ dài nửa đường Vẽ đường vẽ có ghi số 5, 10 và đường vẽ k ghi số
            double x1, x2, y1, y2;//các điểm của đường vẽ từ x1, y1 đến x2, y2
            int indexLine = 0;
            SolidColorBrush WhitePen = new SolidColorBrush(Colors.Green);
            for (int j_setup = 0; j_setup < 12; j_setup++)
                Pitch_LineAutoRemove_setup(j_setup, WhitePen, 2);
            for (double index = -2 * h1 + Pitch * h1 / 10; index <= 2 * h1 + Pitch * h1 / 10; index += h1)
            {
                x1 = StartX - index * Math.Sin(Math.PI * Roll / 180) - R1 * Math.Cos(Math.PI * Roll / 180);
                y1 = StartY + index * Math.Cos(Math.PI * Roll / 180) - R1 * Math.Sin(Math.PI * Roll / 180);
                x2 = StartX - index * Math.Sin(Math.PI * Roll / 180) + R1 * Math.Cos(Math.PI * Roll / 180);
                y2 = StartY + index * Math.Cos(Math.PI * Roll / 180) + R1 * Math.Sin(Math.PI * Roll / 180);
                Pitch_LineAutoRemove(indexLine, x1, y1, x2, y2);
                indexLine++;
            }
            //Vẽ các đường không ghi số
            R2 = R1 / 2;
            for (double index = -1.5 * h1 + (Pitch * h1 / 10); index <= 1.5 * h1 + (Pitch * h1 / 10); index += h1)
            {
                x1 = StartX - index * Math.Sin(Math.PI * Roll / 180) - R2 * Math.Cos(Math.PI * Roll / 180);
                y1 = StartY + index * Math.Cos(Math.PI * Roll / 180) - R2 * Math.Sin(Math.PI * Roll / 180);
                x2 = StartX - index * Math.Sin(Math.PI * Roll / 180) + R2 * Math.Cos(Math.PI * Roll / 180);
                y2 = StartY + index * Math.Cos(Math.PI * Roll / 180) + R2 * Math.Sin(Math.PI * Roll / 180);
                Pitch_LineAutoRemove(indexLine, x1, y1, x2, y2);
                indexLine++;
            }
            //Vẽ đường chân trời qua điểm 0, 0
            //Chỉ số là 9 màu xanh ngày 29/02/2016 bỏ Vẽ đường chân trời qua điểm 0, 0
            /*
            R2 = StartX;
            x1 = StartX - (Pitch * h1 / 10) * Math.Sin(Math.PI * Roll / 180) - R2 * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (Pitch * h1 / 10) * Math.Cos(Math.PI * Roll / 180) - R2 * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (Pitch * h1 / 10) * Math.Sin(Math.PI * Roll / 180) + R2 * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (Pitch * h1 / 10) * Math.Cos(Math.PI * Roll / 180) + R2 * Math.Sin(Math.PI * Roll / 180);
            Pitch_LineAutoRemove(indexLine, WhitePen, 2, x1, y1, x2, y2);
            */
            indexLine++;//đổi chỉ số đường khác cho đường tiếp theo
                        //Vẽ String ở hai bên
                        //Vẽ String
                        //Ghi chữ
                        //-10 là độ dời chữ lên trên
                        //+ 22 là dời sang trái
                        //x1 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(0, Roll, "20", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(1, Roll, "20", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(2, Roll, "10", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(3, Roll, "10", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 15) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 15) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(4, Roll, "0", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(5, Roll, "0", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(6, Roll, "10", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(7, Roll, "10", 16, WhitePen, x2, y2, 1);
                        ////********************
                        //x1 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
                        //y1 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
                        //x2 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
                        //y2 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
                        //Pitch_SetupString_AutoRemove(8, Roll, "20", 16, WhitePen, x1, y1, 1);
                        //Pitch_SetupString_AutoRemove(9, Roll, "20", 16, WhitePen, x2, y2, 1);
                        //Vẽ string ở bên phải
            x1 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (-2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(0, Roll, "20", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(1, Roll, "20", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (-h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (-h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(2, Roll, "10", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(3, Roll, "10", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 15) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 15) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - ((Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + ((Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(4, Roll, "0", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(5, Roll, "0", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(6, Roll, "10", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(7, Roll, "10", 16, WhitePen, x2, y2, 1);
            //********************
            x1 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) - (R1 + 22) * Math.Cos(Math.PI * Roll / 180);
            y1 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) - (R1 + 22) * Math.Sin(Math.PI * Roll / 180);
            x2 = StartX - (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Sin(Math.PI * Roll / 180) + (R1 + 2) * Math.Cos(Math.PI * Roll / 180);
            y2 = StartY + (2 * h1 + (Pitch * h1 / 10) - 10) * Math.Cos(Math.PI * Roll / 180) + (R1 + 2) * Math.Sin(Math.PI * Roll / 180);
            //Pitch_SetupString_AutoRemove(8, Roll, "20", 16, WhitePen, x1, y1, 1);
            Pitch_SetupString_AutoRemove(9, Roll, "20", 16, WhitePen, x2, y2, 1);

            //Ngày 21/12/2015 Vẽ góc Pitch
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            R2 = 2 * R1;
            x1 = StartX - R2;
            y1 = StartY;
            //Vẽ đường ngang của Pitch bằng line
            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, 16, WhitePen, x2, y2, 1);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(0, new SolidColorBrush(Colors.Yellow), x1 - 30, y1 - 3, 30, 8, 1);
            //Vẽ một chấm đỏ hình chữ nhật ngay trung tâm
            Pitch_Draw_Rect_AutoRemove(2, new SolidColorBrush(Colors.Red), StartX - 4, y1 - 3, 8, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(0, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 + 8, y1 + 2, 1);

            indexLine++;
            //*****************************************
            x1 = StartX + R2;
            y1 = StartY;

            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, x2, y2);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 3, 30, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 - 8, y1 + 2, 1);

            //

        }
        //************************************************************************************
        //**************************************************************************
        //Ngày 20/12/2015 Vẽ góc Pitch
        /// <summary>
        /// Vẽ góc Pitch, Roll của máy bay
        /// Với StartX, StartY là điểm trung tâm
        /// h1: khoảng cách giữa 2 đường
        /// </summary>
        /// <param name="Pitch"></param>
        /// <param name="StartX"></param>
        /// <param name=""></param>
        void Pitch_Draw_Angle(double Pitch, double Roll, double StartX, double StartY, double h1)
        {
            double R1 = 40, R2;//R1, R2 là độ dài nửa đường Vẽ đường vẽ có ghi số 5, 10 và đường vẽ k ghi số
            double x1, x2, y1, y2;//các điểm của đường vẽ từ x1, y1 đến x2, y2
            int indexLine = 0;
            double temp, temp1, temp2, temp3, temp4, temp5, temp6; //bien temp de giam tinh toan
            //Cách 2, giữ nguyên đường màu vàng mỗi lần góc Pitch thay đổi thì toàn bộ các đường song song 
            //chạy xuống hoặc chạy lên
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            //Pitch - ((int)Pitch / 10) * 10: pitch = 22.76, (int)Pitch = 22, ((int)Pitch / 10) * 10 = 20;
            //SolidColorBrush WhitePen = new SolidColorBrush(Colors.Green);
            temp3 = ((int)Pitch / 10) * 10;
            temp = (Pitch - temp3) * h1 / 10;
            temp1 = Math.Sin(Math.PI * Roll / 180);
            temp2 = Math.Cos(Math.PI * Roll / 180);
            for (double index = -2 * h1 + temp; index <= 2 * h1 + temp; index += h1)
            {
                x1 = StartX - index * temp1 - R1 * temp2;
                y1 = StartY + index * temp2 - R1 * temp1;
                x2 = StartX - index * temp1 + R1 * temp2;
                y2 = StartY + index * temp2 + R1 * temp1;
                Pitch_LineAutoRemove(indexLine, x1, y1, x2, y2);
                indexLine++;
            }
            //Vẽ các đường không ghi số
            R2 = R1 / 2;
            for (double index = -1.5 * h1 + temp; index <= 1.5 * h1 + temp; index += h1)
            {
                x1 = StartX - index * temp1 - R2 * temp2;
                y1 = StartY + index * temp2 - R2 * temp1;
                x2 = StartX - index * temp1 + R2 * temp2;
                y2 = StartY + index * temp2 + R2 * temp1;
                Pitch_LineAutoRemove(indexLine, x1, y1, x2, y2);
                indexLine++;
            }

            indexLine++;//đổi chỉ số đường khác cho đường tiếp theo
                        //Vẽ String ở 2 cột
                        ////Vẽ String
                        ////Ghi chữ
                        ////-10 là độ dời chữ lên trên
                        ////+ 22 là dời sang trái, phu thuoc vao so chu so can hien thi, dat bien nay la dodoisangtrai
                        //Int16 dodoisangtrai;
                        //if ((((int)Pitch / 10) * 10 + 20).ToString().Length < 3) dodoisangtrai = 23;
                        //else dodoisangtrai = 28;
                        ////phong xuat hien so 0
                        //if ((((int)Pitch / 10) * 10 + 20).ToString().Length < 2) dodoisangtrai = 15;
                        //x1 = StartX - (-2 * h1 + (temp) - 10) * temp1 - (R1 + dodoisangtrai) * temp2;
                        //y1 = StartY + (-2 * h1 + (temp) - 10) * temp2 - (R1 + dodoisangtrai) * temp1;
                        //x2 = StartX - (-2 * h1 + (temp) - 10) * temp1 + (R1 + 2) * temp2;
                        //y2 = StartY + (-2 * h1 + (temp) - 10) * temp2 + (R1 + 2) * temp1;
                        //Pitch_ChangeString_AutoRemove(0, Roll, (((int)Pitch / 10) * 10 + 20).ToString(), x1, y1);
                        //Pitch_ChangeString_AutoRemove(1, Roll, (((int)Pitch / 10) * 10 + 20).ToString(), x2, y2);
                        ////********************
                        //if ((((int)Pitch / 10) * 10 + 10).ToString().Length < 3) dodoisangtrai = 23;
                        //else dodoisangtrai = 28;
                        //if ((((int)Pitch / 10) * 10 + 10).ToString().Length < 2) dodoisangtrai = 15;
                        //x1 = StartX - (-h1 + (temp) - 10) * temp1 - (R1 + dodoisangtrai) * temp2;
                        //y1 = StartY + (-h1 + (temp) - 10) * temp2 - (R1 + dodoisangtrai) * temp1;
                        //x2 = StartX - (-h1 + (temp) - 10) * temp1 + (R1 + 2) * temp2;
                        //y2 = StartY + (-h1 + (temp) - 10) * temp2 + (R1 + 2) * temp1;
                        //Pitch_ChangeString_AutoRemove(2, Roll, (((int)Pitch / 10) * 10 + 10).ToString(), x1, y1);
                        //Pitch_ChangeString_AutoRemove(3, Roll, (((int)Pitch / 10) * 10 + 10).ToString(), x2, y2);
                        ////********************
                        //if ((((int)Pitch / 10) * 10 + 0).ToString().Length < 3) dodoisangtrai = 23;
                        //else dodoisangtrai = 28;
                        //if ((((int)Pitch / 10) * 10 + 0).ToString().Length < 2) dodoisangtrai = 15;
                        //x1 = StartX - ((temp) - 10) * temp1 - (R1 + dodoisangtrai) * temp2;
                        //y1 = StartY + ((temp) - 10) * temp2 - (R1 + dodoisangtrai) * temp1;
                        //x2 = StartX - ((temp) - 10) * temp1 + (R1 + 2) * temp2;
                        //y2 = StartY + ((temp) - 10) * temp2 + (R1 + 2) * temp1;
                        //Pitch_ChangeString_AutoRemove(4, Roll, (((int)Pitch / 10) * 10).ToString(), x1, y1);
                        //Pitch_ChangeString_AutoRemove(5, Roll, (((int)Pitch / 10) * 10).ToString(), x2, y2);
                        ////********************
                        //if ((((int)Pitch / 10) * 10 - 10).ToString().Length < 3) dodoisangtrai = 23;
                        //else dodoisangtrai = 28;
                        //if ((((int)Pitch / 10) * 10 - 10).ToString().Length < 2) dodoisangtrai = 15;
                        //x1 = StartX - (h1 + (temp) - 10) * temp1 - (R1 + dodoisangtrai) * temp2;
                        //y1 = StartY + (h1 + (temp) - 10) * temp2 - (R1 + dodoisangtrai) * temp1;
                        //x2 = StartX - (h1 + (temp) - 10) * temp1 + (R1 + 2) * temp2;
                        //y2 = StartY + (h1 + (temp) - 10) * temp2 + (R1 + 2) * temp1;
                        //Pitch_ChangeString_AutoRemove(6, Roll, (((int)Pitch / 10) * 10 - 10).ToString(), x1, y1);
                        //Pitch_ChangeString_AutoRemove(7, Roll, (((int)Pitch / 10) * 10 - 10).ToString(), x2, y2);
                        ////********************
                        //if ((((int)Pitch / 10) * 10 - 20).ToString().Length < 3) dodoisangtrai = 23;
                        //else dodoisangtrai = 28;
                        //if ((((int)Pitch / 10) * 10 - 20).ToString().Length < 2) dodoisangtrai = 15;
                        //x1 = StartX - (2 * h1 + (temp) - 10) * temp1 - (R1 + dodoisangtrai) * temp2;
                        //y1 = StartY + (2 * h1 + (temp) - 10) * temp2 - (R1 + dodoisangtrai) * temp1;
                        //x2 = StartX - (2 * h1 + (temp) - 10) * temp1 + (R1 + 2) * temp2;
                        //y2 = StartY + (2 * h1 + (temp) - 10) * temp2 + (R1 + 2) * temp1;
                        //Pitch_ChangeString_AutoRemove(8, Roll, (((int)Pitch / 10) * 10 - 20).ToString(), x1, y1);
                        //Pitch_ChangeString_AutoRemove(9, Roll, (((int)Pitch / 10) * 10 - 20).ToString(), x2, y2);

            //**********************************************************************************************
            //Vẽ string 1 cột bên phải
            //Vẽ String
            //Ghi chữ
            //-10 là độ dời chữ lên trên
            //+ 22 là dời sang trái, phu thuoc vao so chu so can hien thi, dat bien nay la dodoisangtrai

            temp4 = (temp) - 10;
            temp5 = (R1 + 2) * temp2;
            temp6 = (R1 + 2) * temp1;
            x2 = StartX - (-2 * h1 + temp4) * temp1 + temp5;
            y2 = StartY + (-2 * h1 + temp4) * temp2 + temp6;
            //Pitch_ChangeString_AutoRemove(0, Roll, (temp3 + 20).ToString(), x1, y1);
            Pitch_ChangeString_AutoRemove(1, Roll, (temp3 + 20).ToString(), x2, y2);
            //********************
            x2 = StartX - (-h1 + temp4) * temp1 + temp5;
            y2 = StartY + (-h1 + temp4) * temp2 + temp6;
            //Pitch_ChangeString_AutoRemove(2, Roll, (temp3 + 10).ToString(), x1, y1);
            Pitch_ChangeString_AutoRemove(3, Roll, (temp3 + 10).ToString(), x2, y2);
            //********************
            x2 = StartX - (temp4) * temp1 + temp5;
            y2 = StartY + (temp4) * temp2 + temp6;
            //Pitch_ChangeString_AutoRemove(4, Roll, (temp3).ToString(), x1, y1);
            Pitch_ChangeString_AutoRemove(5, Roll, (temp3).ToString(), x2, y2);
            //********************
            x2 = StartX - (h1 + temp4) * temp1 + temp5;
            y2 = StartY + (h1 + temp4) * temp2 + temp6;
            //Pitch_ChangeString_AutoRemove(6, Roll, (temp3 - 10).ToString(), x1, y1);
            Pitch_ChangeString_AutoRemove(7, Roll, (temp3 - 10).ToString(), x2, y2);
            //********************

            x2 = StartX - (2 * h1 + temp4) * temp1 + temp5;
            y2 = StartY + (2 * h1 + temp4) * temp2 + temp6;
            //Pitch_ChangeString_AutoRemove(8, Roll, (temp3 - 20).ToString(), x1, y1);
            Pitch_ChangeString_AutoRemove(9, Roll, (temp3 - 20).ToString(), x2, y2);


        }
        //************************************************************************************
        //Vẽ Line Auto Remove cho Pitch
        void Pitch_LineAutoRemove_setup(int index, SolidColorBrush ColorOfLine, double SizeOfLine)
        {
            //BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            LinePitchAutoRemove[index] = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line LinePitchAutoRemove[index] = new Line();
            //BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            //LinePitchAutoRemove[index].Fill = new SolidColorBrush(Colors.Green);
            LinePitchAutoRemove[index].Stroke = ColorOfLine;
            //LinePitchAutoRemove[index].
            //LinePitchAutoRemove[index].Height = 10;
            //LinePitchAutoRemove[index].Width = 10;

            LinePitchAutoRemove[index].StrokeThickness = SizeOfLine;
            //Xac định tọa độ
            //LinePitchAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            //BackgroundDisplay.Children.Add(LinePitchAutoRemove[index]);

        }
        Line[] LinePitchAutoRemove = new Line[12];
        /// <summary>
        /// Vẽ đường thẳng từ (x1, y1) đến (x2, y2)
        /// Bút vẽ là ColorOfLine
        /// độ rông là SizeOfLine
        /// index: chỉ số của đường
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        void Pitch_LineAutoRemove(int index, double x1, double y1, double x2, double y2)
        {
            BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            //LinePitchAutoRemove[index] = new Line();
            //Điểm bắt đầu trên cùng có tọa độ 0, 0
            //Line LinePitchAutoRemove[index] = new Line();
            //BackgroundDisplay.Children.Remove(LinePitchAutoRemove[index]);
            //LinePitchAutoRemove[index].Fill = new SolidColorBrush(Colors.Green);
            //LinePitchAutoRemove[index].Stroke = ColorOfLine;
            //LinePitchAutoRemove[index].
            //LinePitchAutoRemove[index].Height = 10;
            //LinePitchAutoRemove[index].Width = 10;
            LinePitchAutoRemove[index].X1 = x1;
            LinePitchAutoRemove[index].Y1 = y1;
            LinePitchAutoRemove[index].X2 = x2;
            LinePitchAutoRemove[index].Y2 = y2;
            //LinePitchAutoRemove[index].StrokeThickness = SizeOfLine;
            //Xac định tọa độ
            //LinePitchAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-1500, -200, 0, 0);
            BackgroundDisplay.Children.Add(LinePitchAutoRemove[index]);

        }
        //**************************************************************************
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_Pitch = new TextBlock[10];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// Roll góc nghiêng của string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Pitch_SetupString_AutoRemove(int index, double Roll, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_Pitch[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Pitch[index].Height = HeightOfBlock;
            //Tb_Pitch[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_Pitch[index]);
            Tb_Pitch[index] = new TextBlock();
            Tb_Pitch[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_Pitch[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_Pitch[index].Margin = 
            //
            //đảo chữ
            Tb_Pitch[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_Pitch[index].Text = drawString;
            Tb_Pitch[index].FontSize = SizeOfText;
            Tb_Pitch[index].FontFamily = new FontFamily("Arial");
            //Tb_Pitch[index].FontStyle = "Arial";
            //Tb_Pitch[index].FontStretch
            //color text có độ đục
            //Tb_Pitch[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_Pitch[index].Foreground = Blush;
            Tb_Pitch[index].Opacity = Opacity;
            //Quay Textblock để quay chữ
            Tb_Pitch[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };
            //position of text left, top, right, bottom
            Tb_Pitch[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Pitch[index]);
        }
        //**********************************************************************************************
        /// <summary>
        /// Remove chỗi string cũ
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void Pitch_ChangeString_AutoRemove(int index, double Roll, string drawString,
            double StartX, double StartY)
        {
            //create graphic text block design text
            //TextBlock Tb_Pitch[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_Pitch[index].Height = HeightOfBlock;
            //Tb_Pitch[index].Width = WidthOfBlock;
            //canh lề, left, right, center

            //Tb_Pitch[index].Margin = 
            //

            BackgroundDisplay.Children.Remove(Tb_Pitch[index]);
            //Quay Textblock để quay chữ
            Tb_Pitch[index].RenderTransform = new RotateTransform()
            {
                Angle = Roll,
                //CenterX = 25, //The prop name maybe mistyped 
                //CenterY = 25 //The prop name maybe mistyped 
            };
            Tb_Pitch[index].Text = drawString;


            //Tb_Pitch[index].FontStyle = "Arial";
            //Tb_Pitch[index].FontStretch
            //color text có độ đục
            //Tb_Pitch[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));

            //position of text left, top, right, bottom
            Tb_Pitch[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_Pitch[index]);
        }
        //*********************************************************************************************
        //************************************************************
        //Ngày 14/1/2/2015 22h39 đã hoàn thành việc vẽ tam giác đúng vị trí
        //Hoàn thahf vẽ display cảm biến gia tốc
        Polygon[] Pitch_ArrowAutoRemove = new Polygon[2];
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void Pitch_ArrowAuto_Remove(int index, SolidColorBrush BlushOfTriAngle, double x1, double y1,
            double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));
            //Polygon Pitch_ArrowAutoRemove[index] = new Polygon();
            BackgroundDisplay.Children.Remove(Pitch_ArrowAutoRemove[index]);
            Pitch_ArrowAutoRemove[index] = new Polygon();
            Pitch_ArrowAutoRemove[index].Points = myPointCollection;
            Pitch_ArrowAutoRemove[index].Fill = BlushOfTriAngle;
            Pitch_ArrowAutoRemove[index].Width = (xmax - xmin);
            Pitch_ArrowAutoRemove[index].Height = (ymax - ymin);
            Pitch_ArrowAutoRemove[index].Stretch = Stretch.Fill;
            Pitch_ArrowAutoRemove[index].Stroke = BlushOfTriAngle;
            Pitch_ArrowAutoRemove[index].Opacity = Opacity;
            Pitch_ArrowAutoRemove[index].StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2158 + Pitch_ArrowAutoRemove[index].Width - (200 - 2 * xmin), -600 + Pitch_ArrowAutoRemove[index].Height, 0, 0);
            //Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + Pitch_ArrowAutoRemove[index].Height + 4 * slider.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //Pitch_ArrowAutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(Pitch_ArrowAutoRemove[index]);

        }
        //Bước đột phá, tạo mảng hình chữ nhật auto remove biến này là biến toàn cục
        Rectangle[] Pitch_Ret_AutoRemove = new Rectangle[3];

        //Set up for rectangle
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Pitch_Draw_Rect_AutoRemove(int index, SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(Pitch_Ret_AutoRemove[index]);
            Pitch_Ret_AutoRemove[index] = new Rectangle();
            Pitch_Ret_AutoRemove[index].Fill = Blush;
            Pitch_Ret_AutoRemove[index].Height = height;
            Pitch_Ret_AutoRemove[index].Width = width;
            Pitch_Ret_AutoRemove[index].Opacity = Opacity;
            //Xac định tọa độ
            Pitch_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + Pitch_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + Pitch_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(Pitch_Ret_AutoRemove[index]);

        }
        //********************************************************************************
        //Ngày 21/12/2015 vẽ góc Pitch And Roll
        /// <summary>
        /// Set up góc Pitch and Roll
        /// Pitch: Pitch Angle Of Flight
        /// Roll: Roll Angle Of Flight
        /// CenterX, CenterY: Center Coordinate
        /// Radious: Radious of arc to Draw Roll Angle
        /// DisTwoLine: Distance between two line in display of Pitch Angle
        /// </summary>
        /// <param name="Pitch"></param>
        /// <param name="Roll"></param>
        /// <param name="CenterX"></param>
        /// <param name="CenterY"></param>
        /// <param name="Radious"></param>
        /// <param name="DisTwoLine"></param>
        void PitchAndRoll_Setup(double Pitch, double Roll, double CenterX, double CenterY, double Radious, double DisTwoLine)
        {
            RollAngle_Setup(Roll, CenterX, CenterY, Radious);
            //PitchAngle_Setup(Pitch, -Roll, CenterX, CenterY, DisTwoLine);
            Background_Pitch_Roll_Setup(0, 0, CenterX, CenterY);
        }
        //*****************************************************************
        //********************************************************************************
        //Ngày 21/12/2015 vẽ góc Pitch And Roll
        /// <summary>
        /// Set up góc Pitch and Roll
        /// Pitch: Pitch Angle Of Flight
        /// Roll: Roll Angle Of Flight
        /// CenterX, CenterY: Center Coordinate
        /// Radious: Radious of arc to Draw Roll Angle
        /// DisTwoLine: Distance between two line in display of Pitch Angle
        /// </summary>
        /// <param name="Pitch"></param>
        /// <param name="Roll"></param>
        /// <param name="CenterX"></param>
        /// <param name="CenterY"></param>
        /// <param name="Radious"></param>
        /// <param name="DisTwoLine"></param>
        void PitchAndRoll_Draw(double Roll, double Pitch, double CenterX, double CenterY, double Radious, double DisTwoLine)
        {
            RollAngle_Draw_TriAngle(Roll, CenterX, CenterY, Radious);//đã tối ưu ngày 3/3/2016
            //Pitch_Draw_Angle(Pitch, Roll, CenterX, CenterY, DisTwoLine);//đã tối ưu ngày 4/3/2016 lúc 0h 39
            //Ngày 12/3/2016 xoay hình
            Draw_RollAndPitch_optimize(Roll, Pitch, CenterX, CenterY);
        }
        //*****************************************************************
        /// <summary>
        /// Set up vị trí cho cảm biến đo Vertical Speed
        /// dVerticalSpeed: tốc độ máy bay theo chiều dọc tầm từ -2000 đến 2000 km/h
        /// PoinStart_X, PoinStart_Y: tọa độ bắt đầu hình chữ nhật chứa toàn bộ phần cần hiển thị
        /// </summary>
        /// <param name="dVerticalSpeed"></param>
        /// <param name="PoinStart_X"></param>
        /// <param name="PoinStart_Y"></param>

        void VerticalSpeed_Setup(double dVerticalSpeed, double PoinStart_X, double PoinStart_Y)
        {
            //Biến fVerticalSpeed luôn bằng 0, chỉ vẽ trong đoạn -2 đến 2
            double fVerticalSpeed = 0;
            //Graphics formGraphic = this.CreateGraphics();

            //Create pens.
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            // Draw lines between original points to screen.
            //formGraphic.DrawLines(redPen, curvePoints);
            double Width = 8, Height = 200;
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //Chi chữ Vertical Speed
            DrawString("Vert_Speed", 22, new SolidColorBrush(Colors.Red), PoinStart_X - 30, PoinStart_Y + Height + 15, 1);
            DrawString("(Km/h)", 22, new SolidColorBrush(Colors.Red), PoinStart_X - 5, PoinStart_Y + Height + 45, 1);
            //Diem bat dau ben trai la PoinStart_X
            //Draw BackGround
            //Số -14 để bao hết vệt đen bên trên
            //Số + 14 để bao màu đen bên dưới
            FillRectangle(BlushRectangle2, PoinStart_X, PoinStart_Y - 14, 70 + Width,
                (PoinStart_Y + Height + 14) - (PoinStart_Y - 14));

            //Ve duong vien mau trang
            //Point[] curvePoints = { new Point(PoinStart_X, PoinStart_Y - 10), new Point(PoinStart_X + Width + 62,
            //    PoinStart_Y - 10), new Point(PoinStart_X + Width + 62 , PoinStart_Y + Height + 8),
            //                      new Point(PoinStart_X, PoinStart_Y + Height + 8)};
            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePoints);
            DrawLine(whitePen, 1, PoinStart_X, PoinStart_Y - 10, PoinStart_X, PoinStart_Y + Height + 8);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Font drawFont = new Font("Arial", 12);
            //SolidBrush drawBrush = new SolidBrush(Color.White);
            //Khoang cach giua diem cao nhat va thap nhat va chia lam bao nhieu khoang
            //double dSizeOfText = 16;
            Int16 Index_Resolution = 4, I16FullScale = 4, So_Khoang_Chia = 4;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / So_Khoang_Chia)
            {
                //-1 de duong ke ngang co do rong phu hop tai vi tri muon ke.
                DrawLine(whitePen, 1, PoinStart_X, AirSpeed_Index - 1, PoinStart_X + 15, AirSpeed_Index - 1);
                //Ghi chu chi do phan giai
                /*Phep toan lam tron den do phan giai can hien thi, hang tram iDoPhanGiai = I16FullScale / So_Khoang_Chia = 100;
                *(Int16)fVerticalSpeed / (Int16)iDoPhanGiai * (Int16)iDoPhanGiai :so nay o  vi tri trung tam
                */
                /*So o vi tri thap nhat
                 * (Int16)fVerticalSpeed / (Int16)iDoPhanGiai * (Int16)iDoPhanGiai - I16FullScale / 2
                 */
                /*
                 * Vi tri bat dau cua chu la sau vat ke vi tri tai: (PoinStart_X + 15, AirSpeed_Index - 10)
                 * So -10 de canh chinh chu cho phu hop
                 */
                //Viet tu tren xuong duoi chi so y tang dan: AirSpeed_Index += (double)Height / So_Khoang_Chia
                //nhung chi so do cao giam dan: Index_Resolution -= (Int16)iDoPhanGiai;
                DrawString(((Int16)fVerticalSpeed / (Int16)iDoPhanGiai * (Int16)iDoPhanGiai - I16FullScale / 2 +
                    Index_Resolution).ToString(), 16, whitePen, PoinStart_X + 15, AirSpeed_Index - 10, 1);
                Index_Resolution -= (Int16)iDoPhanGiai;
            }
            //Ke nhung doan ngan hon va khong ghi so
            for (double AirSpeed_Index = PoinStart_Y + Height / (2 * So_Khoang_Chia); AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / So_Khoang_Chia)
            {
                DrawLine(whitePen, 1, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X, AirSpeed_Index - 1);
            }
            //Ve Cho mau den de ghi Airspeed hang nghin va hang tram
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(Int16)(PoinStart_X + 15)
             */
            //Full Scale là 4000
            //Int16 Config_Position = 12, DoRongVungDen = 36, i16StartStrAxisX;
            //i16StartStrAxisX = (Int16)(PoinStart_X + 15);
            //formGraphic.FillRectangle(Brushes.Black, i16StartStrAxisX, PoinStart_Y - Config_Position +
            //    (2000 - (Int16)f_Global_VerticalSpeed) * Height / 4000, DoRongVungDen, Config_Position * 2);
            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)f_Global_VerticalSpeed / 100
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)f_Global_VerticalSpeed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)f_Global_VerticalSpeed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)f_Global_VerticalSpeed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)f_Global_VerticalSpeed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */
            //Int16 SizeOfString = 16;
            //Full Scale là 4000

            //drawFont = new Font("Arial", SizeOfString);
            //formGraphic.DrawString(((Int16)f_Global_VerticalSpeed / 100).ToString(), drawFont, drawBrush, PoinStart_X + 15,
            //                    PoinStart_Y - 12 + (2000 - (Int16)f_Global_VerticalSpeed) * Height / 4000);
            //Ve mau den hang don vi va hang chuc
            /*Vị trí bắt đầu: i16StartStrAxisX + DoRongVungDen
             * Vị trí trên trục y: (Int16)((I16FullScale / 2) - (Int16)f_Global_VerticalSpeed % 100) * Height / I16FullScale
             * Lùi trục y 1 khoang SizeOfString để chữ trung tâm nằm giữa màu đen
             * Độ cao màu đen viết được 2 số theo chiều cao: SizeOfString * 2
             * Độ rông vạch đen bằng 26
             */

            //Số +3 để dịch màu đen xuống phía dưới đặt ngay trung tâm
            //VerSpeed_Rect_Setup(0, BlushRectangle4, i16StartStrAxisX - 36 + DoRongVungDen, PoinStart_Y + 3 - SizeOfString +
            //(Int16)((I16FullScale / 2) - (Int16)dVerticalSpeed) * Height / I16FullScale, 63, 25, 1);
            //Vẽ mũi tên

            //Draw_TriAngle_Var(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den làm tròn đến hàng chục
            /* Vị tri chu bat dau i16StartStrAxisX + DoRongVungDen
             * Lấy chữ số hàng chục: ((Int16)f_Global_VerticalSpeed % 100 / 10)
             * Doi trung tâm chữ về bên trái DoiChu = 3 đơn vị
             * Lệnh DrawString chữ bắt đầu tại PointX - Font_X / 4 = PointX - 12/ 4 = PointX - 3;
             * Cách xác định vị trí hàng chục:
             * Vị trí hàng trăm PoinStart_Y - 12 + ((I16FullScale / 2) - (Int16)f_Global_VerticalSpeed % 100) * Height / I16FullScale
             * Cũng chính là vị trí hàng chục nếu hàng đơn vị bằng 0, ta chỉ thể hiện 3 số hàng chục kế nhau tại vạch màu đen
             * Nếu hàng đơn vị khác 0 ta sẽ dịch chuển vị trí hàng chục đi xuống bằng cách tăng y
             * Nếu hàng đơn vị là 10 thì ta đi được 1 chữ số 16 đơn vị theo trục y
             * Nên nếu dư a đơn vị ((Int16)f_Global_VerticalSpeed % 10) ta dịch 1 khoảng (Int16)f_Global_VerticalSpeed % 10 * 16 / 10)
             * 
             */
            /********************************************************************
             * Viết chữ trong phạm vi cho phép
             *     e.Graphics.DrawString(drawString, drawFont, drawBrush, drawRect, drawFormat);
             *     RectangleF drawRect = new RectangleF(x, y, width, height);
             * Điểm bắt đầu màu đen nhỏ là (i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
             * (Int16)((I16FullScale / 2) - (Int16)f_Global_VerticalSpeed % 100) * Height / I16FullScale)
             * Điểm kết thúc (i16StartStrAxisX + DoRongVungDen, PoinStart_Y - SizeOfString +
             * (Int16)((I16FullScale / 2) - (Int16)f_Global_VerticalSpeed % 100) * Height / I16FullScale + 26)
             */
            //Int16 StartOfHChuc, i16DoiChu = 3;
            //StartOfHChuc = (Int16)(i16StartStrAxisX + DoRongVungDen - i16DoiChu);
            //drawFont = new Font("Arial", SizeOfString);
            //Vị trí bắt đầu của chuỗi số chục đơn vị StartStrY
            /*Bắt đầu màu đen (StartBlackX, StartBlackY)
             * Kết thúc màu đen (StartBlackX, EndBlackY)
             * 
             */
            //Ngày 03/10/2015
            //Int32 StartBlackX = StartOfHChuc + 4;
            //Int32 StartBlackY = PoinStart_Y - SizeOfString +
            //    (Int16)((I16FullScale / 2) - (Int16)f_Global_VerticalSpeed) * Height / I16FullScale;
            //Int32 EndBlackY = PoinStart_Y - SizeOfString +
            //    (Int16)((I16FullScale / 2) - (Int16)f_Global_VerticalSpeed) * Height / I16FullScale + SizeOfString * 2;
            //Int32 StartStrY = PoinStart_Y - 8 + ((I16FullScale / 2)
            //    - (Int16)f_Global_VerticalSpeed) * Height / I16FullScale + (Int16)f_Global_VerticalSpeed % 10 * 16 / 10;
            //Vẽ hình chữ nhật bao chữ lại
            //Vì canh lề trên nên ta dời hình chữ nhật lên trên 4 đơn vị nên có  số 4 xuất hiện
            //Số +7 là độ dời chữ ngay trung tâm theo chiều cao
            //Rectangle drawRect1 = new Rectangle(StartBlackX - 45, StartBlackY + 7, 72, (20));
            //formGraphic.DrawRectangle(Pens.Red, drawRect1);

            // Set format of string.
            //StringFormat drawFormat = new StringFormat();
            //drawFormat.Alignment = StringAlignment.Far;
            //drawFormat.LineAlignment = StringAlignment.Center;
            //formGraphic.DrawString(((Int16)f_Global_VerticalSpeed).ToString(), drawFont, drawBrush,
            //    drawRect1, drawFormat);
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            //Còn bên notepad
            I16FullScale = 4000;
            double Config_Position = 12, i16StartStrAxisX;
            i16StartStrAxisX = (PoinStart_X + 15);

            double SizeOfString = 24;
            //Thay đổi độ rộng vùng đen
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = dVerticalSpeed.ToString().Length * (SizeOfString * 0.6);
            if (dVerticalSpeed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (dVerticalSpeed.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (dVerticalSpeed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Vẽ hình chữ nhật mà đen để hiện số tự động remove khi hình mới xuất hiện
            //Hình chữ nhật màu đen của Alt có chỉ số là 0
            VerSpeed_Rect_Setup(0, BlushRectangle4, i16StartStrAxisX, PoinStart_Y - Config_Position - 1 +
                    (I16FullScale / 2 - (Int32)dVerticalSpeed) * Height / I16FullScale, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX;
            y1 = PoinStart_Y - Config_Position +
                (I16FullScale / 2 - (Int32)dVerticalSpeed) * Height / I16FullScale;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X;
            y3 = (y1 + y2) / 2;
            //Altitude_Draw_TriAngle(x1, y1, x2, y2, x3, y3);
            VerSpeed_Poly_AutoRemove(BlushRectangle4, x1, y1, x2, y2, x3, y3, 1);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)fAltitude / 100
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)fAltitude % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)fAltitude / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)fAltitude % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)fAltitude % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //drawFont = new Font("Arial", SizeOfString);
            //DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
            //PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);
            //Chữ trong màu đen có chỉ số là 0
            //VerSpeed_SetupString_AutoRemove(0, dVerticalSpeed.ToString(), PoinStart_X + 15,
            //                    PoinStart_Y - 15 + (300 - dVerticalSpeed % 100) * Height / I16FullScale, 1);
            //Int32 StartBlackY = PoinStart_Y - SizeOfString +
            //    (Int16)((I16FullScale / 2) - (Int16)dVerticalSpeed) * Height / I16FullScale;

            VerSpeed_SetupString_AutoRemove(0, dVerticalSpeed.ToString(), SizeOfString, whitePen, PoinStart_X + 15,
                    PoinStart_Y - 15 + (I16FullScale / 2 - (Int32)dVerticalSpeed) * Height / I16FullScale, 1);

            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        }
        //*******************************************************************
        //*****************************************************************
        /// <summary>
        /// Set up vị trí cho cảm biến đo Vertical Speed
        /// dVerticalSpeed: tốc độ máy bay theo chiều dọc tầm từ -2000 đến 2000 km/h
        /// PoinStart_X, PoinStart_Y: tọa độ bắt đầu hình chữ nhật chứa toàn bộ phần cần hiển thị
        /// </summary>
        /// <param name="dVerticalSpeed"></param>
        /// <param name="PoinStart_X"></param>
        /// <param name="PoinStart_Y"></param>

        void VerticalSpeed_Draw_VerSpeed(double dVerticalSpeed, double PoinStart_X, double PoinStart_Y)
        {
            //Biến fVerticalSpeed luôn bằng 0, chỉ vẽ trong đoạn -2 đến 2
            //double fVerticalSpeed = 0;
            //Graphics formGraphic = this.CreateGraphics();

            //Create pens.
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);

            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            // Draw lines between original points to screen.
            //formGraphic.DrawLines(redPen, curvePoints);
            double Height = 200;

            //double dSizeOfText = 16;
            Int16 I16FullScale = 4, So_Khoang_Chia = 4;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;

            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            //Còn bên notepad
            I16FullScale = 4000;
            double Config_Position = 12, i16StartStrAxisX;
            i16StartStrAxisX = (PoinStart_X + 15);

            double SizeOfString = 24;
            //Thay đổi độ rộng vùng đen
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = dVerticalSpeed.ToString().Length * (SizeOfString * 0.6);
            if (dVerticalSpeed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (dVerticalSpeed.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (dVerticalSpeed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Vẽ hình chữ nhật mà đen để hiện số tự động remove khi hình mới xuất hiện
            //Hình chữ nhật màu đen của Alt có chỉ số là 0
            VerSpeed_Rect_Setup(0, BlushRectangle4, i16StartStrAxisX, PoinStart_Y - Config_Position - 1 +
                    (I16FullScale / 2 - (Int32)dVerticalSpeed) * Height / I16FullScale, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX;
            y1 = PoinStart_Y - Config_Position +
                (I16FullScale / 2 - (Int32)dVerticalSpeed) * Height / I16FullScale;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X;
            y3 = (y1 + y2) / 2;
            //Altitude_Draw_TriAngle(x1, y1, x2, y2, x3, y3);
            VerSpeed_Poly_AutoRemove(BlushRectangle4, x1, y1, x2, y2, x3, y3, 1);

            VerSpeed_SetupString_AutoRemove(0, dVerticalSpeed.ToString(), SizeOfString, whitePen, PoinStart_X + 15,
                    PoinStart_Y - 15 + (I16FullScale / 2 - (Int32)dVerticalSpeed) * Height / I16FullScale, 1);

            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        }
        //*******************************************************************
        //Bước đột phá, tạo mảng hình chữ nhật auto remove biến này là biến toàn cục
        Rectangle[] VerSpeed_Ret_AutoRemove = new Rectangle[2];

        //Set up for rectangle
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void VerSpeed_Rect_Setup(int index, SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(VerSpeed_Ret_AutoRemove[index]);
            VerSpeed_Ret_AutoRemove[index] = new Rectangle();
            VerSpeed_Ret_AutoRemove[index].Fill = Blush;
            VerSpeed_Ret_AutoRemove[index].Height = height;
            VerSpeed_Ret_AutoRemove[index].Width = width;
            VerSpeed_Ret_AutoRemove[index].Opacity = Opacity;
            //Xac định tọa độ
            VerSpeed_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + VerSpeed_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + VerSpeed_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(VerSpeed_Ret_AutoRemove[index]);

        }
        //********************************************************************************

        /// <summary>
        /// Khi tác động vào hình chữ nhật thì remove cái cũ và add vị trí mới
        /// màu sắc và độ đục đã được set up rồi
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// index là số thứ tự hình chữ nhật auto remove
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void VerSpeed_Rect_Change_AutoRemove(int index, double StartX, double StartY, double width, double height)
        {
            //Rectangle TestRetangle = new Rectangle();
            BackgroundDisplay.Children.Remove(VerSpeed_Ret_AutoRemove[index]);
            VerSpeed_Ret_AutoRemove[index].Height = height;
            VerSpeed_Ret_AutoRemove[index].Width = width;
            //Xac định tọa độ
            VerSpeed_Ret_AutoRemove[index].Margin = new Windows.UI.Xaml.Thickness(
                    -2358 + dConvertToTabletX + VerSpeed_Ret_AutoRemove[index].Width + StartX * 2, -798 + dConvertToTabletY + VerSpeed_Ret_AutoRemove[index].Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(VerSpeed_Ret_AutoRemove[index]);

        }
        //********************************************************************************
        Polygon VerSpeed_PolygonAutoRemove = new Polygon();
        /// <summary>
        /// Vẽ hình tam giác, giải thuật trong vở đồ án
        /// tam giác cũ sẽ tự remove khi tam giác mới xuất hiện
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        void VerSpeed_Poly_AutoRemove(SolidColorBrush BlushOfTriAngle, double x1, double y1, double x2, double y2, double x3, double y3, double Opacity)
        {
            double xmin, ymin, xmax, ymax;
            xmin = Math.Min(x1, Math.Min(x2, x3));
            ymin = Math.Min(y1, Math.Min(y2, y3));
            xmax = Math.Max(x1, Math.Max(x2, x3));
            ymax = Math.Max(y1, Math.Max(y2, y3));
            //Draw

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point((x1 - xmin), (y1 - ymin)));
            myPointCollection.Add(new Point(x2 - xmin, y2 - ymin));
            myPointCollection.Add(new Point(x3 - xmin, y3 - ymin));
            //myPointCollection.Add(new Point(0.025, 0.005 * slider.Value));
            //Polygon VerSpeed_PolygonAutoRemove = new Polygon();
            BackgroundDisplay.Children.Remove(VerSpeed_PolygonAutoRemove);
            VerSpeed_PolygonAutoRemove.Points = myPointCollection;
            VerSpeed_PolygonAutoRemove.Fill = BlushOfTriAngle;
            VerSpeed_PolygonAutoRemove.Width = (xmax - xmin);
            VerSpeed_PolygonAutoRemove.Height = (ymax - ymin);
            VerSpeed_PolygonAutoRemove.Stretch = Stretch.Fill;
            VerSpeed_PolygonAutoRemove.Stroke = BlushOfTriAngle;
            VerSpeed_PolygonAutoRemove.Opacity = Opacity;
            VerSpeed_PolygonAutoRemove.StrokeThickness = 1;
            //Xac định tọa độ -1856, -491 là dời về 0, 0
            //quá trình khảo sát trong vở
            //VerSpeed_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2158 + VerSpeed_PolygonAutoRemove.Width - (200 - 2 * xmin), -600 + VerSpeed_PolygonAutoRemove.Height, 0, 0);
            //VerSpeed_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 +  dConvertToTabletX + xmax + xmin, -600 + VerSpeed_PolygonAutoRemove.Height + 4 * slider.Value, 0, 0);
            //Quá trình khảo sát y và tính sai số
            VerSpeed_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xmax + xmin, -800 + dConvertToTabletY + ymax + ymin, 0, 0);
            //VerSpeed_PolygonAutoRemove.Margin = new Windows.UI.Xaml.Thickness(-2060, -491, 0, 0);
            BackgroundDisplay.Children.Add(VerSpeed_PolygonAutoRemove);

        }
        //*********************************************************************************************
        //*****************************************************************
        //Ngày 20/12/2015 bước đột phá tạo một mảng TextBlock Auto remove
        TextBlock[] Tb_VerSpeed = new TextBlock[1];
        //Có căn lề phải
        //Vẽ trong hình chữ nhật
        //**************************************************************************************************
        /// <summary>
        /// Chuỗi đưa vào drawString
        /// Font là Arial, 
        /// Size drawFont
        /// Color Blush
        /// Vị trí StartX, StartY
        /// Set up init location for string
        /// index: index of string
        /// </summary>
        /// <param name="drawString"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        public void VerSpeed_SetupString_AutoRemove(int index, string drawString, double SizeOfText, SolidColorBrush Blush,
            double StartX, double StartY, double Opacity)
        {
            //create graphic text block design text
            //TextBlock Tb_VerSpeed[index] = new TextBlock();
            //chiều dài rộng của khung chứa text
            //Tb_VerSpeed[index].Height = HeightOfBlock;
            //Tb_VerSpeed[index].Width = WidthOfBlock;
            //canh lề, left, right, center
            BackgroundDisplay.Children.Remove(Tb_VerSpeed[index]);
            Tb_VerSpeed[index] = new TextBlock();
            Tb_VerSpeed[index].HorizontalAlignment = HorizontalAlignment.Left;
            Tb_VerSpeed[index].VerticalAlignment = VerticalAlignment.Top;
            //Tb_VerSpeed[index].Margin = 
            //
            //đảo chữ
            Tb_VerSpeed[index].TextWrapping = Windows.UI.Xaml.TextWrapping.NoWrap;
            Tb_VerSpeed[index].Text = drawString;
            Tb_VerSpeed[index].FontSize = SizeOfText;
            Tb_VerSpeed[index].FontFamily = new FontFamily("Arial");
            //Tb_VerSpeed[index].FontStyle = "Arial";
            //Tb_VerSpeed[index].FontStretch
            //color text có độ đục
            //Tb_VerSpeed[index].Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 0, 255, 0));
            Tb_VerSpeed[index].Foreground = Blush;
            Tb_VerSpeed[index].Opacity = Opacity;
            //position of text left, top, right, bottom
            Tb_VerSpeed[index].Margin = new Windows.UI.Xaml.Thickness(StartX + 2, StartY, 0, 0);
            BackgroundDisplay.Children.Add(Tb_VerSpeed[index]);
        }
        //**********************************************************************************************

        //*********************************************************************************************


        /// <summary>
        /// Xảy ra ngắt khi có sự thay đổi giá trị của slider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //dx1 = 10 * slider.Value;
            //dx2 = 500 + 3 * slider.Value;
            //dy1 = 9 * slider.Value;
            //dy2 = 400 + 3 * slider.Value;
            //draw_Balance();
            //Draw_TriAngle();
            //PolygonAutoRemove(dx1, dy1, dx2, dy2, dx3, dy3);

            //Background_Sensor();
            //ComPass_Draw_Compass(350, 550, 100, slider.Value);
            //Altitude_Setup(slider.Value + 178, 500, 100);
            //Altitude_Draw_Alt(slider.Value + 178, 500, 100);
            //Speed_Draw_Speed(slider.Value, 200, 100);
            //double dBalance_mid_X = 400, dBalance_mid_Y = 200, dBalance_R = 120;
            //RollAngle_Draw_TriAngle(slider.Value, 350, 200, 140);
            //Pitch_Draw_Angle(slider.Value, slider.Value, 350, 200, 50);
            //PitchAndRoll_Draw(slider.Value, 30, 350, 200, 140, 50);

            //Vẽ sự thay đổi
            //Speed_Draw_Speed((slider.Value + 60) * 3, 150, 100);
            //Altitude_Draw_Alt((slider.Value + 60) * 3 + 123, 550, 100);
            //VerticalSpeed_Draw_VerSpeed(slider.Value * 15, 550 + i16EditPosition  * 17 / 6, 420);
            //PitchAndRoll_Draw(slider.Value / 6, slider.Value, 350, 200, 140, 50);
            //ComPass_Draw_Compass((slider.Value + 60) * 3, 350 + i16EditPosition  * 11 / 6, 500, 120);
        }


        //**************************************************###################################################
        void DrawArcVietDoAn(double x, double y, double width, double height, int startAngle, int sweepAngle)

        {


            RingSlice TestRinslice = new RingSlice();
            //BackgroundDisplay.Children.Remove(TestRinslice);
            TestRinslice.StartAngle = (double)startAngle + 90;
            TestRinslice.EndAngle = startAngle + 90 + sweepAngle;
            TestRinslice.Fill = new SolidColorBrush(Colors.Green);
            TestRinslice.Radius = height / 2;
            TestRinslice.InnerRadius = height / 2 - 30;
            //Thickness sẽ dời tâm đường tròn

            TestRinslice.Margin = new Windows.UI.Xaml.Thickness(
                -2357 + (TestRinslice.Radius + x) * 2, -799 + (TestRinslice.Radius + y) * 2, 0, 0);
            BackgroundDisplay.Children.Add(TestRinslice);

        }
        //**************************************************###################################################



        //Write text to a file
        async Task saveStringToLocalFile1(string filename, string content)
        {
            // saves the string 'content' to a file 'filename' in the app's local storage folder
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());

            // create a file with the given filename in the local folder; replace any existing file with the same name
            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            // write the char array created from the content string into the file

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                stream.Write(fileBytes, 0, fileBytes.Length);
            }
        }
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //************************************************************************************
        //********************Đồ Án 2*********************************************************
        //Ngày 29/12/2015 Vẽ quỹ đạo bay
        //draw line in map 2D
        //Vẽ quỹ đạo là nét liền nên ta dùng 2 biến tạm để lưu giá trị cũ của Lat, Lon
        double old_Lat, old_Lon;
        //Vẽ quỹ đạo
        Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline_AddFlight = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
        /// <summary>
        /// Chấm điểm có màu vàng tại vị trí lat, lon, Alt
        /// Vẽ vị trí máy bay và góc quay của máy bay
        /// Vẽ đường thẳng nối tới điểm đích
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <param name="dHeading"></param>
        void Draw_Trajectory_And_Flight(double lat, double lon, double alt, double dHeading)
        {
            //double centerLatitude = myMap.Center.Position.Latitude;
            //double centerLongitude = myMap.Center.Position.Longitude;



            //************************Vẽ Máy bay***********************************
            myMap.Children.Remove(img);


            //Edit size of image
            img.Height = 5 * myMap.ZoomLevel;
            img.Width = 5 * myMap.ZoomLevel;

            //img.RenderTransform
            img.Opacity = 0.7;


            //img.Transitions.
            img.Source = new BitmapImage(new Uri("ms-appx:///Assets/airplane-icon.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            img.RenderTransform = new RotateTransform()
            {

                Angle = dHeading,
                CenterX = 10 * myMap.ZoomLevel / 2,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 10 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img.sca
            img.Stretch = Stretch.Uniform;
            img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            img.Margin = new Windows.UI.Xaml.Thickness(-10 * myMap.ZoomLevel / 2, -10 * myMap.ZoomLevel / 2, 0, 0);




            Geopoint Position = new Geopoint(new BasicGeoposition()
            {
                Latitude = lat,
                Longitude = lon,
                //Altitude = 200.0
            });
            //myMap.Children.Add(bitmapImage);
            //Dặ Ảnh đúng vị trí
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(img, Position);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(img, new Point(0.5, 0.5));
            if (old_Lat != 0.0)//Vì lúc đầu chưa có dữ liệu nên k hiện máy bay
                myMap.Children.Add(img);

            //Vẽ quỹ đạo
            //Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
            mapPolyline.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude = old_Lat, Longitude = old_Lon, Altitude = alt + 0.00005},
                //San Bay Tan Son Nhat
                new BasicGeoposition() {Latitude = lat, Longitude = lon, Altitude = alt - 0.00005},
            });

            mapPolyline.StrokeColor = Colors.Red;
            mapPolyline.StrokeThickness = 2;
            mapPolyline.StrokeDashed = false;//nét liền
            if (old_Lat != 0.0)//Vì lúc đầu chưa có dữ liệu nên k hiện máy bay
            {
                myMap.MapElements.Add(mapPolyline);

                //Ve duong thang den dentination
                //San bay tan son nhat:  dLatDentination, dLonDentination google map
                Map_DrawLine_2D(lat, lon, 10.818345, 106.658897);
            }
            //Updata giá trí mới
            old_Lat = lat;
            old_Lon = lon;

        }
        //*****************************************
        //Ngày 04/03/2016 tối ưu vẽ máy bay
        //********************Đồ Án 2*********************************************************
        //Ngày 29/12/2015 Vẽ quỹ đạo bay
        //draw line in map 2D
        //Vẽ quỹ đạo là nét liền nên ta dùng 2 biến tạm để lưu giá trị cũ của Lat, Lon
        //double old_Lat, old_Lon;
        /// <summary>
        /// Chấm điểm có màu vàng tại vị trí lat, lon, Alt
        /// Vẽ vị trí máy bay và góc quay của máy bay
        /// Vẽ đường thẳng nối tới điểm đích
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="alt"></param>
        /// <param name="dHeading"></param>
        void Draw_Trajectory_And_Flight_optimize(double lat, double lon, double alt, double dHeading)
        {
            //double centerLatitude = myMap.Center.Position.Latitude;
            //double centerLongitude = myMap.Center.Position.Longitude;



            //************************Vẽ Máy bay***********************************
            myMap.Children.Remove(img);


            //Edit size of image
            img.Height = 10 * myMap.ZoomLevel;
            img.Width = 10 * myMap.ZoomLevel;

            //img.RenderTransform
            //img.Opacity = 0.7;


            //img.Transitions.
            //img.Source = new BitmapImage(new Uri("ms-appx:///Assets/airplane-icon.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            img.RenderTransform = new RotateTransform()
            {

                Angle = dHeading,
                CenterX = 10 * myMap.ZoomLevel / 2,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 10 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img.sca
            //img.Stretch = Stretch.Uniform;
            //img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            //img.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            img.Margin = new Windows.UI.Xaml.Thickness(-10 * myMap.ZoomLevel / 2, -10 * myMap.ZoomLevel / 2, 0, 0);




            Geopoint Position = new Geopoint(new BasicGeoposition()
            {
                Latitude = lat,
                Longitude = lon,
                //Altitude = 200.0
            });
            //myMap.Children.Add(bitmapImage);
            //Dặ Ảnh đúng vị trí
            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(img, Position);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(img, new Point(0.5, 0.5));
            //if (old_Lat != 0.0)//Vì lúc đầu chưa có dữ liệu nên k hiện máy bay
            //    myMap.Children.Add(img);

            //Vẽ quỹ đạo
            //Windows.UI.Xaml.Controls.Maps.MapPolyline mapPolyline = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
            mapPolyline.Path = new Geopath(new List<BasicGeoposition>() {
                new BasicGeoposition() {Latitude = old_Lat, Longitude = old_Lon},
                //San Bay Tan Son Nhat
                new BasicGeoposition() {Latitude = lat, Longitude = lon},
            });

            //mapPolyline.StrokeColor = Colors.Red;
            //mapPolyline.StrokeThickness = 2;
            //mapPolyline.StrokeDashed = false;//nét liền
            if (old_Lat != 0.0)//Vì lúc đầu chưa có dữ liệu nên k hiện máy bay
            {
                myMap.MapElements.Add(mapPolyline);
                myMap.Children.Add(img);

                //Ve duong thang den dentination
                //San bay tan son nhat:  dLatDentination, dLonDentination google map
                Map_DrawLine_2D(lat, lon, dLatDentination, dLonDentination);
            }
            //Updata giá trí mới
            old_Lat = lat;
            old_Lon = lon;

        }
        //*****************************************
        //*******************************************************
        //Ngày 10/1/2016
        /// <summary>
        /// Hiện thị các dữ liệu lên giao diện map
        /// </summary>
        /// <param name="dPitch"></param>
        /// <param name="dRoll"></param>
        /// <param name="dSpeed"></param>
        /// <param name="dAlt"></param>
        /// <param name="dVerSpeed"></param>
        /// <param name="dAngle"></param>
        void DisplayDataOnMap(double dPitch, double dRoll, double dSpeed,
                        double dAlt, double dVerSpeed, double dAngle)
        {
            //Vẽ sự thay đổi
            //PitchAndRoll_Draw(dPitch, dRoll, 350, 200, 140, 50);
            //Speed_Draw_Speed(dSpeed, 150, 100);
            //
            //Draw_Airspeed_optimize(dSpeed, 150 - 32, 225);
            //Luôn cho vận tốc >= 0, độ cao >= 0
            if (dSpeed < 0) dSpeed = 0;
            if (dAlt < 0)   dAlt = 0;
            Draw_Airspeed_full_optimize(dSpeed, 150 - 32 + i16EditPosition, 205);//ok

            //Altitude_Draw_Alt(dAlt, 550, 100);
            Draw_Alttitude_full_optimize(dAlt, 550 + 88 / 2 + i16EditPosition  * 17 / 6, 80);//ok

            VerticalSpeed_Draw_VerSpeed(dVerSpeed, 550 + i16EditPosition  * 17 / 6, 420);
            //Ngày 03/03/2016 phát hiện cái la bàn để hiển thị góc yaw, còn giá trị góc
            //trước ,T là hướng của vector vận tốc, góc này quan trọng khi máy bay bị trôi
            //ComPass_Draw_Compass(dAngle, 350 + i16EditPosition  * 11 / 6, 500, 120);
        }


        //************************************************************************
        bool bSetup = false;
        bool bOneScreen = false;
        /// <summary>
        /// 1 nut thuc thi 2 chuc nang: One and Two Screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneSceen_Click(object sender, RoutedEventArgs e)
        {
            //myMap.Margin = new Windows.UI.Xaml.Thickness(0, 0, 00, 00);
            bOneScreen = !bOneScreen;
            if (bOneScreen)
            {
                Background_Sensor(00, -80);

                btOneSceen.Content = "Two Screen";
            }
            else
            {
                //Background_Sensor(700, -80);
                DisplaySensor_Setup();
                btOneSceen.Content = "One Screen";
            }
        }
        //*********************************************************************
        Rectangle TestRet_BackGround = new Rectangle();
        //**********************************************************************************************
        /// <summary>
        /// Vẽ Rectangle với bút vẽ brush tọa độ bắt đầu là (x, y) vè chiều rộng Width, chiều cao height
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void FillRect_BackGround(SolidColorBrush Blush, double StartX, double StartY, double width, double height, double Opacity)
        {
            BackgroundDisplay.Children.Remove(TestRet_BackGround);
            TestRet_BackGround.Fill = Blush;
            TestRet_BackGround.Height = height;
            TestRet_BackGround.Width = width;
            TestRet_BackGround.Opacity = Opacity;
            //Xac định tọa độ
            TestRet_BackGround.Margin = new Windows.UI.Xaml.Thickness(
                -2358 + dConvertToTabletX + TestRet_BackGround.Width + StartX * 2, -798 + dConvertToTabletY + TestRet_BackGround.Height + StartY * 2, 0, 0);
            //TestRetangle.Margin = new Windows.UI.Xaml.Thickness(
            //-2358 + TestRetangle.Width + x * 2, -200, 0, 0);
            BackgroundDisplay.Children.Add(TestRet_BackGround);
        }
        //********************************************************************************
        //Ngay 20/1/2016
        //Tinh goc giua 2 diem
        public double angleFromCoordinate(double lat1, double long1, double lat2,
        double long2)
        {
            //convert degrees to rad
            lat1 = lat1 * Math.PI / 180;
            long1 = long1 * Math.PI / 180;
            lat2 = lat2 * Math.PI / 180;
            long2 = long2 * Math.PI / 180;
            double dLon = (long2 - long1);
            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1)
                    * Math.Cos(lat2) * Math.Cos(dLon);
            double brng = Math.Atan2(y, x);
            //Convert to Degrees
            brng = brng * 180 / Math.PI;
            brng = (brng + 360) % 360;
            //brng = 360 - brng;
            return Math.Round(brng, 2);
        }

        /// <summary>
        /// Set up các giao diện
        /// Remove các phần không cần thiết
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setup_Click(object sender, RoutedEventArgs e)
        {
            //Set up
            FillRect_BackGround(new SolidColorBrush(Colors.Blue), 0, -300, 700,
            1000 - dConvertToTabletY, 0.4);
            DisplaySensor_Setup();

            bSetup = true;
        }

        //*******************************************************************
        /// <summary>
        /// Zoom map to flight's location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindFight_Click(object sender, RoutedEventArgs e)
        {
            myMap.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle San Bay Tan Son Nhat: dLatDentination, dLonDentination
                   Latitude = dLatGol,
                   Longitude = dLonGol
               });
            //myMap.ZoomLevel = 16;
        }

        //*******************************************************************
        //Test
        //double dx1 = 200, dy1 = 100, dx2 = 300, dy2 = 400, dx3 = 400, dy3 = 300;
        /// <summary>
        /// Test các hàm vừa được viết
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestFunction_Click(object sender, RoutedEventArgs e)
        {


        }
        /// <summary>
        /// save content to C:\Users\VANCHUC-PC\AppData\Local\Packages\
        /// 54fa2b45-b04f-4b40-809b-7556c7ed473f_pq4mhrhe9d4xp\LocalState\dataReceive.txt
        /// Note: Close dataReceive.txt before write
        /// </summary>
        /// <param name="content"></param>
        public void SaveTotxt(string content)
        {
            //Windows.Storage.FileIO.ReadLinesAsync()
            //Windows.Storage.FileIO.ReadLinesAsync()
            Windows.Storage.FileIO.AppendTextAsync(sampleFile, content);
            //Delete buffer
            //bufferSavedata = "";

            //c2
            //using (StreamReader sr = new StreamReader()
            //{
            //    while (sr.Peek() >= 0)
            //    {
            //        Console.WriteLine(sr.ReadLine());
            //        //StreamReader.
            //    }
            //}
        }

        //c2
        // READ FILE
        //public async void ReadFile()
        //{
        //    // settings
        //    var path = @"E:\Studying\InSchool\HocKi8\DoAn2\Ket Qua\DataAnhHuan_15_04_2016\data.txt";
        //    var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;


        //    // acquire file
        //    var file = await folder.GetFileAsync(path);
        //    //var readFile = await Windows.Storage.FileIO.ReadLinesAsync(file);
        //    //foreach (var line in readFile)
        //    {
        //        //tb_ShowTime.Text = (line);
        //    }
        //}

        //c3
        async void ReadFromFile(string content)
        {
            //Windows.Storage.FileIO.ReadLinesAsync()
            //Windows.Storage.FileIO.ReadLinesAsync()
            //string text = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
            //Delete buffer
            //bufferSavedata = "";

            //c2
            //using (StreamReader sr = new StreamReader()
            //{
            //    while (sr.Peek() >= 0)
            //    {
            //        Console.WriteLine(sr.ReadLine());
            //        //StreamReader.
            //    }
            //}

            StorageFile Fragendatei = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Fragen/frage1.txt"));
            IList<String> inhalt = await Windows.Storage.FileIO.ReadLinesAsync(Fragendatei);
        }

        //**********************************************************************
        //Ngày 11/3/2016
        //Quay ảnh cho real time
        //Ảnh này là biến toàn cục vì nó có được sử dụng trong chưa trình ngắt
        Image img_rotate = new Image();
        public void Image_Edit()
        {

            myMap.Children.Remove(img_rotate);
            //Image img_rotate = sender as Image;
            //BitmapImage bitmapImage = new BitmapImage();
            //img_rotate.Width = bitmapImage.DecodePixelWidth = 80; //natural px width of image source
            // don't need to set Height, system maintains aspect ratio, and calculates the other
            // dimension, so long as one dimension measurement is provided
            // bitmapImage.UriSource = new Uri(img_rotate.BaseUri, "F:/Entertainment/Photo/picnic/test.jpg");

            //BitmapImage bitmapImage = new BitmapImage();
            //Uri uri = new Uri("ms-appx:///Assets/airplane-icon.png");
            //bitmapImage.UriSource = uri;

            // OR
            //airplane-icon.png có kích thước 129 x 129


            //BitmapImage bitmapImage = new BitmapImage();
            //img_rotate.Width = bitmapImage.DecodePixelWidth = 15 * myMap.ZoomLevel;
            //bitmapImage.UriSource = new Uri("ms-appx:///Assets/airplane-icon.png");
            //img_rotate.Source = bitmapImage;
            //myMap.ClearValue();

            //Edit size of image
            img_rotate.Height = 15 * myMap.ZoomLevel;
            img_rotate.Width = 15 * myMap.ZoomLevel;

            //img_rotate.RenderTransform
            img_rotate.Opacity = 0.7;


            //img_rotate.Transitions.
            img_rotate.Source = new BitmapImage(new Uri("ms-appx:///Assets/MyHome.png"));
            Image img_rotate1 = new Image();
            //img_rotate.RenderTransform
            img_rotate1.Opacity = 0.7;


            //img_rotate.Transitions.
            img_rotate1.Source = new BitmapImage(new Uri("ms-appx:///Assets/airplane-icon.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            img_rotate.RenderTransform = new RotateTransform()
            {

                //Angle = 3.6 * slider.Value,
                //CenterX = 15 * myMap.ZoomLevel / 2,
                //CenterX = 62, //The prop name maybe mistyped 
                //CenterY = 15 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img_rotate.sca
            img_rotate.Stretch = Stretch.Uniform;
            img_rotate.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img_rotate.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            img_rotate.Margin = new Windows.UI.Xaml.Thickness(-15 * myMap.ZoomLevel / 2, -15 * myMap.ZoomLevel / 2, 0, 0);
            //tbOutputText.Text = "Latitude: " + myMap.Center.Position.Latitude.ToString() + '\n';
            //tbOutputText.Text += "Longitude: " + myMap.Center.Position.Longitude.ToString() + '\n';
            ////tbOutputText.Text += "Timer Fly: " + Timer_fly.ToString();



            Geopoint PointCenterMap = new Geopoint(new BasicGeoposition()
            {
                Latitude = myMap.Center.Position.Latitude,
                Longitude = myMap.Center.Position.Longitude,
                //Altitude = 200.0
            });
            //myMap.Children.Add(bitmapImage);

            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(img_rotate, PointCenterMap);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(img_rotate, new Point(0.5, 0.5));
            myMap.Children.Add(img_rotate);

            //Vẽ quỹ đạo
            //Draw_Trajectory(Convert.ToDouble(Data.Latitude), Convert.ToDouble(Data.Longtitude), Convert.ToDouble(Data.Altitude));

        }
        //*************************************************************
        //Vẽ hiển thị của cảm biến
        Image imgAuto_test = new Image();
        /// <summary>
        /// top là độ dài lên trên hay xuốn dưới, xCenter, yCenter là trung tâm ảnh khi chưa cắt
        /// dRoll góc xoay
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="top"></param>
        public void Background_Pitch_Roll_Setup(double dRoll, double dPitch, double xCenter, double yCenter)
        {
            double top = -dPitch * 4;
            //BackgroundDisplay.Children.Remove(imgAuto_test);
            //Edit size of image
            imgAuto_test.Height = 1120;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 350 x 1120;
            BackgroundDisplay.Children.Remove(imgAuto_test);
            imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
            imgAuto_test.Width = 350;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_test.RenderTransform
            imgAuto_test.Opacity = 0.5;
            //tbShowDis.Text = imgAuto_test.Height.ToString() + " " + imgAuto_test.Width.ToString();
            //double cutY = 200;
            //imgAuto_test.Height = 1120;
            //top = 560 - 100;//560 là trung tâm y của ảnh
            //imgAuto_test.Clip = new RectangleGeometry()

            //    {
            //    Rect = new Rect(95, 460 + top, 160, 200)//các trên trung tâm y 100, dưới 100

            //    };

            //imgAuto_test.Transitions.

            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            //imgAuto_test.RenderTransform = new RotateTransform()
            //{

            //    Angle = dRoll,
            //    CenterX = 175,
            //    //CenterX = 62, //The prop name maybe mistyped 
            //    CenterY = 560 + top //The prop name maybe mistyped 
            //};
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //imgAuto_test.sca



            //Geopoint PointCenterMap2 = new Geopoint(new BasicGeoposition()
            //{
            //    Latitude = myMap.Center.Position.Latitude + 0.001,
            //    Longitude = myMap.Center.Position.Longitude + 0.001,
            //    //Altitude = 200.0
            //});
            //myMap.Children.Add(bitmapImage);

            //Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(imgAuto_test, PointCenterMap);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(imgAuto_test, new Point(0.5, 0.5));
            //myMap.Children.Add(imgAuto_test);
            ////tbOutputText.Background.
            //ckground.a
            //thu bản đồ lại
            //myMap.Height = 500;
            //Delete các cổng com
            //ConnectDevices.IsEnabled = false;
            //myMap.Children.Remove(ConnectDevices);
            //Background tên là BackgroundDisplay
            //Khi nhấn nút chia 2 màn hình chia là 2 phần
            //phần trái là cảm biến phần bên phải là map
            //chỉnh lại vị trí của ảnh
            //imgAuto_test.Margin = new Windows.UI.Xaml.Thickness(1500 - 2000, -cutY, 00, 00);
            //đã kiểm tra ok
            //dời lên top đơn vị thì  - 2 * top
            imgAuto_test.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_test);

            //Background background_forImage = new Background();
            //slider.ValueChanged += Slider_ValueChanged;
            //Vẽ đường chân trời
            //*************************************************************************
            //Cách 2, giữ nguyên đường màu vàng mỗi lần góc Pitch thay đổi thì toàn bộ các đường song song 
            //chạy xuống hoặc chạy lên
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            double R1 = 40, R2;//R1, R2 là độ dài nửa đường Vẽ đường vẽ có ghi số 5, 10 và đường vẽ k ghi số
            double x1, y1;//các điểm của đường vẽ từ x1, y1 đến x2, y2
            int indexLine = 0;
            SolidColorBrush WhitePen = new SolidColorBrush(Colors.Green);


            //Ngày 21/12/2015 Vẽ góc Pitch
            //h1 <--> 10 degree: (- Pitch * h1 / 10)
            R2 = 2 * R1;
            x1 = xCenter - R2;
            y1 = yCenter;
            //Vẽ đường ngang của Pitch bằng line
            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, 16, WhitePen, x2, y2, 1);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(0, new SolidColorBrush(Colors.Yellow), x1 - 30, y1 - 3, 30, 8, 1);
            //Vẽ một chấm đỏ hình chữ nhật ngay trung tâm
            Pitch_Draw_Rect_AutoRemove(2, new SolidColorBrush(Colors.Red), xCenter - 4, y1 - 3, 8, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(0, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 + 8, y1 + 2, 1);

            indexLine++;
            //*****************************************
            x1 = xCenter + R2;
            y1 = yCenter;

            //Pitch_LineAutoRemove(indexLine, new SolidColorBrush(Colors.Yellow), 8, x1, y1, x2, y2);
            //Bằng Rectangle tốt hơn
            Pitch_Draw_Rect_AutoRemove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 3, 30, 8, 1);
            //Vẽ mũi tên hình tam giác tại đầu mũi đường cho đẹp
            Pitch_ArrowAuto_Remove(1, new SolidColorBrush(Colors.Yellow), x1, y1 - 2, x1, y1 + 6, x1 - 8, y1 + 2, 1);

            //


        }
        /// <summary>
        /// //////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //Draw_RollAndPitch_optimize(-slider.Value, slider.Value, 350, 200);//góc picth < 10 degree code đúng
            //PitchAndRoll_Draw(0, slider.Value, 350, 200, 140, 50);
            //AirSpeed_Image_Setup(0, 140 - 32, 225);
            //Draw_Airspeed_optimize(slider.Value, 140 - 32, 225);
            //AirSpeed_Image_Setup(0, 150 - 32, 100 + 125);//đã canh đúng trung tâm
            //Draw_Airspeed_full_optimize(slider.Value, 150 - 32 + i16EditPosition, 205);
            //Alttitude_Image_full_Setup(slider.Value, 560 + 88 / 2, 100);

            //Draw_Alttitude_full_optimize(slider.Value, 550 + 88 / 2 + i16EditPosition  * 17 / 6, 100);
            //Altitude_Setup(00, 550, 100);
            //myMap.Heading = slider.Value;
        }
        ///////////////////////////////////////////////////////////////////
        //optimize
        /// <summary>
        /// Vẽ Roll and Pitch bằng clip hình
        /// Đã tối ưu ngày 12/3/2016
        /// xCenter, yCenter là trung tâm hình.
        /// </summary>
        /// <param name="dRoll"></param>
        /// <param name="dPitch"></param>
        /// <param name="xCenter"></param>
        /// <param name="yCenter"></param>
        public void Draw_RollAndPitch_optimize(double dRoll, double dPitch, double xCenter, double yCenter)
        {
            double top, t_cut;
            t_cut = -dPitch * 4;
            if (dPitch > 10)
            {
                dPitch = 10 + (dPitch - 10) / 2;
                //t_cut = -(dPitch * 4;
                //top = -dPitch * 2;
            }
            //else

            top = -dPitch * 4;
            //BackgroundDisplay.Children.Remove(imgAuto_test);
            //Edit size of image
            imgAuto_test.Height = 1120;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 350 x 1120;
            BackgroundDisplay.Children.Remove(imgAuto_test);
            //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
            imgAuto_test.Width = 350;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_test.RenderTransform
            //imgAuto_test.Opacity = 0.6;
            //tbShowDis.Text = imgAuto_test.Height.ToString() + " " + imgAuto_test.Width.ToString();
            //double cutY = 200;
            //imgAuto_test.Height = 1120;
            //top = 560 - 100;//560 là trung tâm y của ảnh
            imgAuto_test.Clip = new RectangleGeometry()

            {
                Rect = new Rect(95, 460 + t_cut, 160, 200)//các trên trung tâm y 100, dưới 100

            };

            //imgAuto_test.Transitions.

            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            imgAuto_test.RenderTransform = new RotateTransform()
            {

                Angle = dRoll,
                CenterX = 175,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 560 + t_cut //The prop name maybe mistyped 
            };

            imgAuto_test.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_test);


            //(((((((((((((((((((((((((((((((((((((((((((((((
            //Đường chân trời
            //BackgroundDisplay.Children.Remove(Pitch_Ret_AutoRemove[0]);
            //BackgroundDisplay.Children.Add(Pitch_Ret_AutoRemove[0]);
            //BackgroundDisplay.Children.Remove(Pitch_Ret_AutoRemove[1]);
            //BackgroundDisplay.Children.Add(Pitch_Ret_AutoRemove[1]);
            BackgroundDisplay.Children.Remove(Pitch_Ret_AutoRemove[2]);
            BackgroundDisplay.Children.Add(Pitch_Ret_AutoRemove[2]);
            //Mũi tên
            //BackgroundDisplay.Children.Remove(Pitch_ArrowAutoRemove[0]);
            //BackgroundDisplay.Children.Add(Pitch_ArrowAutoRemove[0]);
            //BackgroundDisplay.Children.Remove(Pitch_ArrowAutoRemove[1]);
            //BackgroundDisplay.Children.Add(Pitch_ArrowAutoRemove[1]);

        }
        ////////////////////////////////////////////////////////////////////////////////
        //Vẽ ảnh air speed
        void Speed_Image_Setup(double Air_Speed, double PoinStart_X, double PoinStart_Y)
        {
            Air_Speed = 0;
            PoinStart_Y = 50;
            //PoinStart_X, PoinStart_Y là góc trên cùng bên phải của hình chữ nhật to

            //Test ok lúc 0h38 ngày 19/12/2015
            //Graphics formGraphic = this.CreateGraphics();
            //Create pens.
            //Pen redPen = new Pen(Color.Red, 3);
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            // Draw lines between original points to screen.
            //formGraphic.DrawLines(redPen, curvePoints);
            //Các bút vẽ cần thiết
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);

            //Tọa độ của Hình vẽ
            //Width, Height là độ rộng và cao của vạch xanh
            double Index_Resolution = 140;
            double Width = 8, Height = Index_Resolution / 60 * 250;
            //Viết chữ Speed + đơn vị km/h
            //DrawString("Speed (Km/h)", 30, new SolidColorBrush(Colors.Green), PoinStart_X - 110, PoinStart_Y + Height + 5, 0.8);
            //Draw BackGround độ đục là 1

            //Delete cái gì vẽ trước đó
            //Ngày 16/12/2015 15h22 đã test ok
            //if (Air_Speed.ToString().Length >= Speed_Old.ToString().Length)
            FillRectangleHaveOpacity(new SolidColorBrush(Colors.Blue), PoinStart_X - 80, PoinStart_Y - 10, 80 + Width,
                    Height + 20, 0.2);

            //Speed_Old = Air_Speed;
            //Ve cot cao hinh chu nhat
            //Ngày 16/12/2015 15h22 đã test ok
            FillRectangle(BlushRectangle3, PoinStart_X, PoinStart_Y - 10, Width, Height + 20);
            //Ve duong vien mau trang



            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePoints);



            DrawLine(BlushOfLine1, 1, PoinStart_X + Width, PoinStart_Y - 10,
                PoinStart_X + Width, PoinStart_Y + Height + 10);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Ngày 16/12/2015 15h22 đã test ok
            //Cỡ chữ 16 bên map là cỡ chữ 12 bên System.Drawing
            //


            double temp = Index_Resolution;
            double temp2 = Index_Resolution / 10;
            //int indexMode = 0;
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height; AirSpeed_Index += (double)Height / temp2)
            {
                DrawLine(BlushOfLine1, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X + Width - 15, AirSpeed_Index - 1);
                //Ghi chu chi do phan giai
                //Ngày 16/12/2015 15h22 đã test ok
                //indexMode++;
                Speed_ModeAutoRemove(((Int16)Air_Speed / 10 * 10 - temp / 2 + Index_Resolution).ToString(),
                    16, BlushOfString1, PoinStart_X - 40, AirSpeed_Index - 10, 1, 8);
                Index_Resolution -= 10;
            }
            for (double AirSpeed_Index = PoinStart_Y + Height / (temp2 * 2); AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / temp2)
            {
                DrawLine(BlushOfLine1, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X, AirSpeed_Index - 1);
            }


        }

        //Vẽ hiển thị của cảm biến
        Image imgAuto_airSpeed = new Image();
        Image imgAuto_airSpeed2 = new Image();
        Image imgAuto_airSpeed3 = new Image();
        Image imgAuto_airSpeed4 = new Image();
        Image imgAuto_airSpeed5 = new Image();
        Image imgAuto_airSpeed6 = new Image();
        /// <summary>
        /// top là độ dài lên trên hay xuốn dưới, xCenter, yCenter là trung tâm ảnh khi chưa cắt
        /// dRoll góc xoay
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="top"></param>
        public void AirSpeed_Image_Setup(double dAirSpeed, double xCenter, double yCenter)
        {
            double top = -dAirSpeed;
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
            //Edit size of image
            imgAuto_airSpeed.Height = 600;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
            imgAuto_airSpeed.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_-30-110.PNG"));
            imgAuto_airSpeed.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed.RenderTransform
            imgAuto_airSpeed.Opacity = 1;

            imgAuto_airSpeed.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_airSpeed);
            ///////////////////////////////////////////////////////////////////////////////////////
            //double top = -dAirSpeed;
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
            //Edit size of image
            imgAuto_airSpeed2.Height = 600;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
            imgAuto_airSpeed2.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_60-200.PNG"));
            imgAuto_airSpeed2.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed2.RenderTransform
            imgAuto_airSpeed2.Opacity = 1;

            imgAuto_airSpeed2.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_airSpeed2);
            ///////////////////////////////////////////////////////////////////////////////////////
            //double top = -dAirSpeed;
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
            //Edit size of image
            imgAuto_airSpeed3.Height = 600;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
            imgAuto_airSpeed3.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_150-290.PNG"));
            imgAuto_airSpeed3.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed3.RenderTransform
            imgAuto_airSpeed3.Opacity = 1;

            imgAuto_airSpeed3.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_airSpeed3);
            ///////////////////////////////////////////////////////////////////////////////////////
            //double top = -dAirSpeed;
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
            //Edit size of image
            imgAuto_airSpeed4.Height = 600;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
            imgAuto_airSpeed4.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_240-380.PNG"));
            imgAuto_airSpeed4.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed4.RenderTransform
            imgAuto_airSpeed4.Opacity = 1;

            imgAuto_airSpeed4.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_airSpeed4);
            ///////////////////////////////////////////////////////////////////////////////////////
            //double top = -dAirSpeed;
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
            //Edit size of image
            imgAuto_airSpeed5.Height = 600;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
            imgAuto_airSpeed5.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_330-470.PNG"));
            imgAuto_airSpeed5.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed5.RenderTransform
            imgAuto_airSpeed5.Opacity = 1;

            imgAuto_airSpeed5.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_airSpeed5);
            ///////////////////////////////////////////////////////////////////////////////////////
            //double top = -dAirSpeed;
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
            //Edit size of image
            imgAuto_airSpeed6.Height = 600;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
            imgAuto_airSpeed6.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_420-560.PNG"));
            imgAuto_airSpeed6.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed6.RenderTransform
            imgAuto_airSpeed6.Opacity = 1;

            imgAuto_airSpeed6.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imgAuto_airSpeed6);
            ///////////////////////////////////////////////////////////////////////////////////////


            //slider.ValueChanged += Slider_ValueChanged;
        }
        ///////////////////////////////////////////////////////////////////
        //optimize
        /// <summary>
        /// Vẽ Roll and Pitch bằng clip hình
        /// Đã tối ưu ngày 12/3/2016
        /// xCenter, yCenter là trung tâm hình.
        /// </summary>
        /// <param name="dRoll"></param>
        /// <param name="dPitch"></param>
        /// <param name="xCenter"></param>
        /// <param name="yCenter"></param>
        public void Draw_Airspeed_optimize(double dAirSpeed, double xCenter, double yCenter)
        {
            //yCenter = 225
            double top, dAirSpeed_original = dAirSpeed;
            if (dAirSpeed < 90)
            {
                dAirSpeed = ((int)dAirSpeed / 10 * 10) - 40;//Image 1
                top = -(dAirSpeed * 40 / 9.6);
                //Edit size of image
                imgAuto_airSpeed.Height = 600;
                //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                //size 80 x 600;
                BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
                //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                imgAuto_airSpeed.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                imgAuto_airSpeed.Clip = new RectangleGeometry()

                {
                    Rect = new Rect(0, 168 + top, 80, 264)//các trên trung tâm y 100, dưới 100

                };

                imgAuto_airSpeed.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
                BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
                BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
                BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
                BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
                BackgroundDisplay.Children.Add(imgAuto_airSpeed);
            }
            else
            {
                if (dAirSpeed < 170)
                {
                    dAirSpeed = ((int)dAirSpeed / 10 * 10) - 130;//Image 1
                    top = -(dAirSpeed * 40 / 9.6);
                    //Edit size of image
                    imgAuto_airSpeed2.Height = 600;
                    //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                    //size 80 x 600;
                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
                    //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                    imgAuto_airSpeed2.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                    imgAuto_airSpeed2.Clip = new RectangleGeometry()

                    {
                        Rect = new Rect(0, 168 + top, 80, 264)//các trên trung tâm y 100, dưới 100

                    };

                    imgAuto_airSpeed2.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
                    BackgroundDisplay.Children.Add(imgAuto_airSpeed2);
                }
                else
                {
                    if (dAirSpeed < 260)
                    {
                        dAirSpeed = ((int)dAirSpeed / 10 * 10) - 220;//Image 1
                        top = -(dAirSpeed * 40 / 9.6);
                        //Edit size of image
                        imgAuto_airSpeed3.Height = 600;
                        //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                        //size 80 x 600;
                        BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
                        //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                        imgAuto_airSpeed3.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                        imgAuto_airSpeed3.Clip = new RectangleGeometry()

                        {
                            Rect = new Rect(0, 168 + top, 80, 264)//các trên trung tâm y 100, dưới 100

                        };

                        imgAuto_airSpeed3.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                        BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
                        BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
                        BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
                        BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
                        BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
                        BackgroundDisplay.Children.Add(imgAuto_airSpeed3);
                    }
                    else
                    {
                        if (dAirSpeed < 350)
                        {
                            dAirSpeed = ((int)dAirSpeed / 10 * 10) - 310;//Image 1
                            top = -(dAirSpeed * 40 / 9.6);
                            //Edit size of image
                            imgAuto_airSpeed4.Height = 600;
                            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                            //size 80 x 600;
                            BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
                            //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                            imgAuto_airSpeed4.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                            imgAuto_airSpeed4.Clip = new RectangleGeometry()

                            {
                                Rect = new Rect(0, 168 + top, 80, 264)//các trên trung tâm y 100, dưới 100

                            };

                            imgAuto_airSpeed4.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                            BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
                            BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
                            BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
                            BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
                            BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
                            BackgroundDisplay.Children.Add(imgAuto_airSpeed4);
                        }
                        else
                        {
                            if (dAirSpeed < 440)
                            {
                                dAirSpeed = ((int)dAirSpeed / 10 * 10) - 400;//Image 1
                                top = -(dAirSpeed * 40 / 9.6);
                                //Edit size of image
                                imgAuto_airSpeed5.Height = 600;
                                //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                                //size 80 x 600;
                                BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
                                //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                                imgAuto_airSpeed5.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                                imgAuto_airSpeed5.Clip = new RectangleGeometry()

                                {
                                    Rect = new Rect(0, 168 + top, 80, 264)//các trên trung tâm y 100, dưới 100

                                };

                                imgAuto_airSpeed5.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                                BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
                                BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
                                BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
                                BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
                                BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
                                BackgroundDisplay.Children.Add(imgAuto_airSpeed5);
                            }
                            else
                            {
                                if (dAirSpeed < 530)
                                {
                                    dAirSpeed = ((int)dAirSpeed / 10 * 10) - 490;//Image 1
                                    top = -(dAirSpeed * 40 / 9.6);
                                    //Edit size of image
                                    imgAuto_airSpeed6.Height = 600;
                                    //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                                    //size 80 x 600;
                                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed6);
                                    //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                                    imgAuto_airSpeed6.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                                    imgAuto_airSpeed6.Clip = new RectangleGeometry()

                                    {
                                        Rect = new Rect(0, 168 + top, 80, 264)//các trên trung tâm y 100, dưới 100

                                    };

                                    imgAuto_airSpeed6.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed2);
                                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed3);
                                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed4);
                                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed5);
                                    BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
                                    BackgroundDisplay.Children.Add(imgAuto_airSpeed6);
                                }
                            }
                        }
                    }
                }
            }

            //Speed_Draw_Speed(dSpeed, 150, 100);

            Speed_Draw_String_optimize(dAirSpeed_original, 150, 100);

        }
        ///////////////////////////////////////////////////////////////////
        //Image imSpeedFull = new Image();
        //public void AirSpeed_Image_full_Setup(double dAirSpeed, double xCenter, double yCenter)
        //{
        //    double top = -dAirSpeed;
        //    BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
        //    Edit size of image
        //    imSpeedFull.Height = 2474;
        //    muốn biết kích thước thì dùng paint, kích thước trong paint;
        //    size 85 x 601;
        //    BackgroundDisplay.Children.Remove(imSpeedFull);
        //    imSpeedFull.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_full.PNG"));
        //    imSpeedFull.Width = 80;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

        //    imgAuto_airSpeed.RenderTransform
        //    imSpeedFull.Opacity = 1;

        //    imSpeedFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
        //    BackgroundDisplay.Children.Add(imSpeedFull);
        //    slider.ValueChanged += Slider_ValueChanged;
        //    Speed_Draw_String_setup(130.6, xCenter + 32, yCenter - 125);//ok
        //    Viết chữ Speed + đơn vị km/ h
        //    DrawString("Speed ", 30, new SolidColorBrush(Colors.Green), xCenter + 32 - 80, yCenter - 125 + 250 + 5, 0.8);
        //    DrawString("(Km/h)", 30, new SolidColorBrush(Colors.Green), xCenter + 32 - 80, yCenter - 125 + 250 + 40, 0.8);
        //}

        //Speed từ 0 đến 1000km/h
        /////////////////////////////////////////////////////////////////
        Image imSpeedFull = new Image();
        public void AirSpeed_Image_full_Setup(double dAirSpeed, double xCenter, double yCenter)
        {
            double top = -dAirSpeed;
            BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
            //Edit size of image
            imSpeedFull.Height = 4934;
            //muốn biết kích thước thì dùng paint, kích thước trong paint;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imSpeedFull);
            imSpeedFull.Source = new BitmapImage(new Uri("ms-appx:///Assets/Speed_Full_v2.png"));
            imSpeedFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed.RenderTransform
            //imSpeedFull.Opacity = 1;

            imSpeedFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imSpeedFull);
            //slider.ValueChanged += Slider_ValueChanged;
            Speed_Draw_String_setup(dAirSpeed, xCenter + 32, yCenter - 125);//ok
            //Viết chữ Speed + đơn vị km/ h
            DrawString("Speed ", 30, new SolidColorBrush(Colors.Green), xCenter + 32 - 80, yCenter - 125 + 250 + 5, 0.8);
            DrawString("(Km/h)", 30, new SolidColorBrush(Colors.Green), xCenter + 32 - 80, yCenter - 125 + 250 + 40, 0.8);
        }
        ///////////////////////////////////////////////////////////////////////////////////////
        //optimize
        /// <summary>
        /// Vẽ Roll and Pitch bằng clip hình
        /// Đã tối ưu ngày 12/3/2016
        /// xCenter, yCenter là trung tâm hình.
        /// </summary>
        /// <param name="dRoll"></param>
        /// <param name="dPitch"></param>
        /// <param name="xCenter"></param>
        /// <param name="yCenter"></param>
        public void Draw_Airspeed_full_optimize(double dAirSpeed, double xCenter, double yCenter)
        {
            //làm tròn và chặn không cho nhỏ hơn 0
            if (dAirSpeed < 0) dAirSpeed = 0;
            dAirSpeed = Math.Round(dAirSpeed, 1);
            //yCenter = 225
            //dAirSpeed = 00;
            double top, dAirSpeed_original = dAirSpeed, t_cut;
            t_cut = -(-2168) - dAirSpeed * 4.16;
            //if (dAirSpeed < 90)
            {
                //dAirSpeed = dAirSpeed - 424;//Image 1
                //top = -(dAirSpeed * 40 / 9.6);

                //top = -(-1103) - dAirSpeed * 4.16;

                if (dAirSpeed < 70.5)
                    top = -(-2168) - dAirSpeed * 4.16;
                else top = -(-2168) - 70.5 * 4.16 - (dAirSpeed - 70.5) * 4.167 / 2;

                //top = -(-slider.Value) - 0 * 4.16;

                //top = -(-1103) - dAirSpeed * 4.16; đúng từ 0 đến 100
                //top = -(-1103) - 105.0 * 4.16 - (dAirSpeed - 105) * 4.168 / 2; chạy ok từ 105 đến hết
                //Edit size of image
                imSpeedFull.Height = 4934;
                //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                //size 80 x 600;
                BackgroundDisplay.Children.Remove(imSpeedFull);
                //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                imSpeedFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                imSpeedFull.Clip = new RectangleGeometry()

                {
                    Rect = new Rect(0, 2334 + t_cut, 88, 264)//các trên trung tâm y 100, dưới 100

                };

                imSpeedFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                BackgroundDisplay.Children.Add(imSpeedFull);
            }


            //Speed_Draw_Speed(dSpeed, 150, 100);

            Speed_Draw_String_optimize(dAirSpeed_original, xCenter + 32, 100);

        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        //*************************************************************************
        void Speed_Draw_String_setup(double Air_Speed, double PoinStart_X, double PoinStart_Y)
        {
            //Test ok lúc 0h38 ngày 19/12/2015
            //Graphics formGraphic = this.CreateGraphics();
            //Create pens.
            //Pen redPen = new Pen(Color.Red, 3);
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            // Draw lines between original points to screen.
            //formGraphic.DrawLines(redPen, curvePoints);
            //Các bút vẽ cần thiết
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush BlushOfLine1 = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);

            //Tọa độ của Hình vẽ
            //Width, Height là độ rộng và cao của vạch xanh
            double Width = 8, Height = 250;
            //Draw BackGround độ đục là 1
            //*************************************************************************
            //Ve Cho mau den de ghi Airspeed
            //Ngày 16/12/2015 15h52 đã test ok
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(Int16)(PoinStart_X + 15)
             */
            double SizeOfString = 24;
            double I16FullScale = 60, So_Khoang_Chia = 6;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            //1 ký tự rộng = SizeOfString * 3 / 4;
            double Config_Position = 12, i16StartStrAxisX;
            //double DoRongVungDen = Air_Speed.ToString().Length * (SizeOfString * 3 / 4);
            double DoRongVungDen = 26;
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = Air_Speed.ToString().Length * (SizeOfString * 0.6);
            if (Air_Speed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (Air_Speed.ToString().Length - 1) * (SizeOfString * 0.6) + 5;
                //10 là độ rông dấu chấm
            }
            if (Air_Speed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Hình chữ nhật được căn lề phải
            i16StartStrAxisX = (PoinStart_X - 41);
            double StartXRect = (PoinStart_X - 10);
            FillRect_AutoRemove3(BlushRectangle4, StartXRect - DoRongVungDenRect, PoinStart_Y - Config_Position - 1 +
                (30 - Air_Speed % 10) * Height / 60, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            //Ngày 16/12/2015 15h52 đã test ok
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX + DoRongVungDen;
            y1 = PoinStart_Y - Config_Position +
                (30 - Air_Speed % 10) * Height / 60;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = PoinStart_X + Width;
            y3 = (y1 + y2) / 2;
            Draw_TriAngle_Var(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)Air_Speed / 100
            //Ngày 16/12/2015 15h56 đã test ok
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)Air_Speed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)Air_Speed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)Air_Speed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            //drawFont = new Font("Arial", SizeOfString);
            //-2 là độ dời chữ vào trong thích hợp
            DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
                                PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);

            //Còn bên Notepad++
            //Khó quá bỏ qua

        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        //*************************************************************************
        void Speed_Draw_String_optimize(double Air_Speed, double PoinStart_X, double PoinStart_Y)
        {




            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)Air_Speed / 100
            //Ngày 16/12/2015 15h56 đã test ok
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)Air_Speed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)Air_Speed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)Air_Speed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */
            //Add mũi tên và hình chữ nhật
            BackgroundDisplay.Children.Remove(RetangleAutoRemove3);
            BackgroundDisplay.Children.Add(RetangleAutoRemove3);
            BackgroundDisplay.Children.Remove(myPolygonAutoRemove);
            BackgroundDisplay.Children.Add(myPolygonAutoRemove);
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            //drawFont = new Font("Arial", SizeOfString);
            //-2 là độ dời chữ vào trong thích hợp
            //DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
            //                    PoinStart_Y - 15 + (30) * Height / I16FullScale, 1);

            //Còn bên Notepad++
            //Khó quá bỏ qua
            if (Air_Speed.ToString().Length > 5) TxtDesignAutoRemove.Text = Air_Speed.ToString().Substring(0, 5);
            else TxtDesignAutoRemove.Text = Air_Speed.ToString();
            BackgroundDisplay.Children.Remove(TxtDesignAutoRemove);

            BackgroundDisplay.Children.Add(TxtDesignAutoRemove);

        }
        //*********************************************************************************************
        //Vẽ độ cao máy bay
        //**************************************************************************
        //******************************************************************************
        //Ngày 19/12/2015 Vẽ Altitude
        /// <summary>
        /// Set up vị trí Altitude của cảm biến
        /// Tọa độ đầu của hình chữ nhật là PoinStart_X, PoinStart_Y
        /// </summary>
        /// <param name="fAltitude"></param>
        void Altitude_Image_Setup(double dAltitude, double PoinStart_X, double PoinStart_Y)
        {
            //Graphics formGraphic = this.CreateGraphics();
            PoinStart_Y = 30;
            //Create pens.
            //Pen whitePen = new Pen(Color.White, 2);
            //Pen greenPen = new Pen(Color.Green, 3);
            //Các bút vẽ cần thiết bên windows.UI
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            SolidColorBrush greenPen = new SolidColorBrush(Colors.Green);
            // Draw lines between original podoubles to screen.
            //formGraphic.DrawLines(redPen, curvePodoubles);
            double Index_Resolution = 1400;
            dAltitude = 9800;
            double Width = 8, Height = Index_Resolution / 600 * 250;
            //Viết chữ Altitude
            //DrawString("Altitude (m)", 30, new SolidColorBrush(Colors.Green), PoinStart_X - 30, PoinStart_Y + Height + 5, 0.8);
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //Diem bat dau ben trai la PoinStart_X
            //Draw BackGround
            //Edit bên map ngày 19/12/2015

            {
                Rect_Setup_AutoRemove(1, BlushRectangle1, PoinStart_X, PoinStart_Y - 8, 80 + Width,
                    Height + 16, 0.2);
            }

            //Ve duong vien mau trang

            //formGraphic.DrawLines(new Pen(Color.White, 1), curvePodoubles);
            DrawLine(whitePen, 1, PoinStart_X, PoinStart_Y - 8, PoinStart_X, PoinStart_Y + Height + 8);
            //Chia do phan giai cho cot cao do
            //Viet so len do phan giai
            //Font drawFont = new Font("Arial", 12);
            //SolidBrush drawBrush = new SolidBrush(Color.White);
            FillRectangle(BlushRectangle3, PoinStart_X, PoinStart_Y - 8, Width, Height + 16);
            //Edit bên map
            double dSizeOfText = 16;
            //Khoang cach giua diem cao nhat va thap nhat va chia lam bao nhieu khoang
            double I16FullScale = Index_Resolution, So_Khoang_Chia = Index_Resolution / 100;
            double iDoPhanGiai = I16FullScale / So_Khoang_Chia;
            int index_str = 1;//là chỉ số của string trong mảng Tb_Alt 
            for (double AirSpeed_Index = PoinStart_Y; AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / So_Khoang_Chia)
            {
                //-1 de duong ke ngang co do rong phu hop tai vi tri muon ke.
                DrawLine(whitePen, 3, PoinStart_X, AirSpeed_Index - 1, PoinStart_X + 15, AirSpeed_Index - 1);
                //Ghi chu chi do phan giai
                /*Phep toan lam tron den do phan giai can hien thi, hang tram iDoPhanGiai = I16FullScale / So_Khoang_Chia = 100;
                *dAltitude / iDoPhanGiai * iDoPhanGiai :so nay o  vi tri trung tam
                */
                /*So o vi tri thap nhat
                 * dAltitude / iDoPhanGiai * iDoPhanGiai - I16FullScale / 2
                 */
                /*
                 * Vi tri bat dau cua chu la sau vat ke vi tri tai: (PoinStart_X + 15, AirSpeed_Index - 10)
                 * So -10 de canh chinh chu cho phu hop
                 */
                //Viet tu tren xuong duoi chi so y tang dan: AirSpeed_Index += (double)Height / So_Khoang_Chia
                //nhung chi so do cao giam dan: Index_Resolution -= iDoPhanGiai;
                Alt_Setup_Image_AutoRemove(index_str, ((Int32)dAltitude / (Int32)iDoPhanGiai * (Int32)iDoPhanGiai - I16FullScale / 2 +
                    Index_Resolution).ToString(), dSizeOfText, whitePen, PoinStart_X + 15, AirSpeed_Index - 10, 1);
                Index_Resolution -= iDoPhanGiai;
                //index_str++;
            }
            //Ke nhung doan ngan hon va khong ghi so
            for (double AirSpeed_Index = PoinStart_Y + Height / (So_Khoang_Chia * 2); AirSpeed_Index <= PoinStart_Y + Height;
                AirSpeed_Index += (double)Height / So_Khoang_Chia)
            {
                DrawLine(whitePen, 3, PoinStart_X + Width, AirSpeed_Index - 1, PoinStart_X, AirSpeed_Index - 1);
            }
            //Ve Cho mau den de ghi Airspeed hang nghin va hang tram
            /* font chu 16 rong 12 cao 16
             * Config_Position = 12 de canh chinh vung mau den cho phu hop
             * SizeOfString = 16 chu cao 16 rong 12
             * chieu cao cua vùng đen 2 * Config_Position = 24
             * Số 28 là độ rông của vùng đen bên trái, vùng đen lớn: DoRongVungDen >= (2 * độ rông của 1 chữ = 24)
             * Số 15 là khoảng cách của chữ cách lề bên trái (bắt đầu đường gạch gạch)
             * Bắt đầu của chữ là: i16StartStrAxisX =(PoinStart_X + 15)
             */





        }
        //**************************************************************************
        ///////////////////////////////////////////////////////////////////
        Image imAlttitudeFull = new Image();
        /// <summary>
        /// Find position with  string input or Lat, Lon input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if(tb_Position.Text != "")
            {
                Search_Offline(tb_Position.Text);
            }
            else
            {//nhap dung toa do là việc của button get
                //if(tb_Lat_Search.Text != "" && tb_Lon_Search.Text != "")
                //{
                //    dLatDentination = Convert.ToDouble(tb_Lat_Search.Text);
                //    dLonDentination = Convert.ToDouble(tb_Lon_Search.Text);
                //    //Add My home picture
                //    AddImageAtLatAndLon(dLatDentination, dLonDentination);
                //}
            }
        }

        /// <summary>
        /// nhìn thấy cả máy bay và đích
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomAll_Click(object sender, RoutedEventArgs e)
        {
            myMap.Center =
               new Geopoint(new BasicGeoposition()
               {
                   //Geopoint for Seattle San Bay Tan Son Nhat: dLatDentination, dLonDentination
                   Latitude = (dLatGol + dLatDentination) / 2,
                   Longitude = (dLonGol + dLonDentination) / 2
               });
            //Công thức giữa khoảng cách và zoom level
            //.ZoomLevel = 8.625 - (distance(dLatGol, dLonGol, Convert.ToDouble(Data.Altitude), dLatDentination, dLonDentination, 0) - 200000) / 50000;
            try
            {
                myMap.ZoomLevel = 8.625 - (distance(dLatGol, dLonGol, Convert.ToDouble(Data.Altitude), dLatDentination, dLonDentination, 0) - 200000) / 50000;
            }
            catch
            {

            }
        }

        /// <summary>
        /// xCenter, yCenter là trung tâm ảnh
        /// </summary>
        /// <param name="dAirSpeed"></param>
        /// <param name="xCenter"></param>
        /// <param name="yCenter"></param>
        public void Alttitude_Image_full_Setup(double dAirSpeed, double xCenter, double yCenter)
        {
            //dAirSpeed = 0;
            double top = -(dAirSpeed - 2000);
            //BackgroundDisplay.Children.Remove(imgAuto_airSpeed);
            //Edit size of image
            imAlttitudeFull.Height = 4560;
            //muốn biết kích thước thì dùng paint, kích thước trong paint ;
            //size 85 x 601;
            BackgroundDisplay.Children.Remove(imAlttitudeFull);
            imAlttitudeFull.Source = new BitmapImage(new Uri("ms-appx:///Assets/AltitudeFull.png"));
            imAlttitudeFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)

            //imgAuto_airSpeed.RenderTransform
            imAlttitudeFull.Opacity = 1;

            imAlttitudeFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
            BackgroundDisplay.Children.Add(imAlttitudeFull);
            /////////////////////////////////////////////////////////////////////////////////////////////
            SolidColorBrush BlushRectangle1 = new SolidColorBrush(Colors.Blue);
            SolidColorBrush BlushRectangle2 = new SolidColorBrush(Colors.Brown);
            SolidColorBrush BlushRectangle3 = new SolidColorBrush(Colors.Green);
            SolidColorBrush BlushRectangle4 = new SolidColorBrush(Colors.Black);
            SolidColorBrush whitePen = new SolidColorBrush(Colors.White);
            SolidColorBrush BlushOfString1 = new SolidColorBrush(Colors.White);
            SolidColorBrush greenPen = new SolidColorBrush(Colors.Green);
            double Height = 250;
            double I16FullScale = 600;
            //Vẽ màu đen và mũi tên và string
            //Khúc trên ok
            //Còn bên notepad
            double Config_Position = 12, i16StartStrAxisX;
            i16StartStrAxisX = (xCenter - 88 / 2 + 15);

            double SizeOfString = 24;
            //Thay đổi độ rộng vùng đen
            //Vì độ rộng của dấu . nhỏ hơn các ký tự còn lại nên ta chia 2 trường hợp
            double DoRongVungDenRect = dAirSpeed.ToString().Length * (SizeOfString * 0.6);
            if (dAirSpeed.ToString().IndexOf('.') != -1)
            //Trong airspeed có dấu chấm
            {
                DoRongVungDenRect = (dAirSpeed.ToString().Length - 1) * (SizeOfString * 0.6) + 10;
                //10 là độ rông dấu chấm
            }
            if (dAirSpeed.ToString().Length == 1)
            //Tăng độ rộng màu đen
            {
                DoRongVungDenRect = 40;
                //4 là độ rông dấu chấm
            }
            //Vẽ hình chữ nhật mà đen để hiện số tự động remove khi hình mới xuất hiện
            //Hình chữ nhật màu đen của Alt có chỉ số là 0
            Rect_Setup_AutoRemove(0, BlushRectangle4, i16StartStrAxisX, yCenter - Config_Position - 1 +
                    (300 - dAirSpeed % 100) * Height / 600, DoRongVungDenRect, Config_Position * 2, 1);

            //Vẽ mũi tên
            double x1, y1, x2, y2, x3, y3;
            x1 = i16StartStrAxisX;
            y1 = yCenter - Config_Position +
                (300 - dAirSpeed % 100) * Height / 600;
            x2 = x1;
            y2 = y1 + Config_Position * 2;
            x3 = xCenter - 88 / 2;
            y3 = (y1 + y2) / 2;
            Altitude_Draw_TriAngle(x1, y1, x2, y2, x3, y3);

            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)fAltitude / 100
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)fAltitude % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)fAltitude / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)fAltitude % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)fAltitude % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //drawFont = new Font("Arial", SizeOfString);
            //DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
            //PoinStart_Y - 15 + (30 - Air_Speed % 10) * Height / I16FullScale, 1);
            //Chữ trong màu đen có chỉ số là 0
            Alt_SetupString_AutoRemove(0, dAirSpeed.ToString(), SizeOfString, whitePen, xCenter - 88 / 2 + 15,
                                yCenter - 15 + (300 - dAirSpeed % 100) * Height / I16FullScale, 1);

            //Viết chữ Altitude
            DrawString("Altitude", 30, new SolidColorBrush(Colors.Green), xCenter - 88 / 2 - 10, yCenter + Height + 5, 0.8);
            DrawString("  (m)", 30, new SolidColorBrush(Colors.Green), xCenter - 88 / 2 - 10, yCenter + Height + 35, 0.8);
            //slider.ValueChanged += Slider_ValueChanged;
            //test
            //Speed_Draw_String_setup(130.6, 150, 100);
            //tb_ZoomLevel.Text = "";
        }
        public void Draw_Alttitude_full_optimize(double dAlttitude, double xCenter, double yCenter)
        {
            //làm tròn và chặn không cho nhỏ hơn 0
            if (dAlttitude < 0) dAlttitude = 0;
            dAlttitude = Math.Round(dAlttitude, 1);

            //yCenter = 225
            //dAlttitude = 4000;
            //if(TestText.Text != "")
            //dAlttitude = Convert.ToDouble(TestText.Text);
            double top, dAirSpeed_original = dAlttitude, t_cut;
            t_cut = -(-1980.45) - dAlttitude * 0.416;
            //top = -(dAlttitude - 2000);
            //if (dAirSpeed < 90)
            {
                //dAirSpeed = dAirSpeed - 424;//Image 1
                //top = -(dAirSpeed * 40 / 9.6);

                //top = -(-1103) - dAirSpeed * 4.16;

                if (dAlttitude < 957.21139) top = -(-1980.45) - dAlttitude * 0.416;
                //else top = -(-1980.45) - dAlttitude * 0.415;
                else {
                    top = -(-1980.3) - 957.21139 * 0.416 - (dAlttitude - 957.21139) * 0.4167 / 2;
                    t_cut = -(-1980.3) - dAlttitude * 0.4167;
                }
                //else top = -(-1103) - 105.0 * 4.16 - (dAlttitude - 105) * 4.168 / 2;

                //top = -(-1103) - dAirSpeed * 4.16; đúng từ 0 đến 100
                //top = -(-1103) - 105.0 * 4.16 - (dAirSpeed - 105) * 4.168 / 2; chạy ok từ 105 đến hết
                //Edit size of image
                imAlttitudeFull.Height = 4560;
                //muốn biết kích thước thì dùng paint, kích thước trong paint ;
                //size 80 x 600;
                BackgroundDisplay.Children.Remove(imAlttitudeFull);
                //imgAuto_test.Source = new BitmapImage(new Uri("ms-appx:///Assets/horizon.bmp"));
                imAlttitudeFull.Width = 88;//Ảnh này hình vuông nên Width = Height = min(Height, Width)


                imAlttitudeFull.Clip = new RectangleGeometry()

                {
                    Rect = new Rect(0, 2272 + t_cut, 88, 264)//các trên trung tâm y 100, dưới 100

                };

                imAlttitudeFull.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + xCenter * 2, -798 + dConvertToTabletY + yCenter * 2 - 2 * top, 0, 0);
                BackgroundDisplay.Children.Add(imAlttitudeFull);
            }

            Alttitude_Draw_String_optimize(dAlttitude);
            //Speed_Draw_Speed(dSpeed, 150, 100);

            //Speed_Draw_String_optimize(dAirSpeed_original, 150, 100);
            ///////////////////////////////////////////////////////////

        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        //*************************************************************************
        void Alttitude_Draw_String_optimize(double Alttitude)
        {




            //ghi chu len mau den, ghi hang nghin va hang tram (Int16)Air_Speed / 100
            //Ngày 16/12/2015 15h56 đã test ok
            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            /* cỡ chữ SizeOfString = 16;
             * Số -12 để canh chỉnh số cho phù hợp
             * (Int16)Air_Speed % 100: Lấy phần chục và đơn vị tìm ra vị trí phù hợp
             * Chữ số này nằm ở nửa trên cách đầu trên cùng ((Int16)Air_Speed / 100 * 100 + 300) 1 khoảng
             * Số 300 là 1/2 của fullScale
             * (300 - (Int16)Air_Speed % 100)
             * đổi qua trục tọa độ * Height / 600, 600 la fullScale
             * Chữ bắt đầu tại Point_X - Font_X / 4, Point_Y - Font_Y / 4
             * Trung tam là PoinStart_Y + (300 - (Int16)Air_Speed % 100) * Height / I16FullScale
             * Bắt đầu là PoinY - 16/ 4 = Trung tam - 16 / 2. 16 là cỡ chữ theo Y,
             * PoiY = Trung Tâm + 12;
             */

            //Add mũi tên và hình chữ nhật
            BackgroundDisplay.Children.Remove(Ret_AutoRemove[0]);
            BackgroundDisplay.Children.Add(Ret_AutoRemove[0]);
            BackgroundDisplay.Children.Remove(myPolygonAutoRemove_Alt);
            BackgroundDisplay.Children.Add(myPolygonAutoRemove_Alt);

            //Cỡ chữ 20 bên map là cỡ chữ 16 bên System.Drawing
            //drawFont = new Font("Arial", SizeOfString);
            //-2 là độ dời chữ vào trong thích hợp
            //DrawStringAutoRemove(Air_Speed.ToString(), SizeOfString, BlushOfString1, i16StartStrAxisX - 0,
            //                    PoinStart_Y - 15 + (30) * Height / I16FullScale, 1);

            //Còn bên Notepad++
            //Khó quá bỏ qua
            //BackgroundDisplay.Children.Add(Tb_Alt[0]);
            BackgroundDisplay.Children.Remove(Tb_Alt[0]);
            if ((Alttitude < 1000) && (Alttitude.ToString().IndexOf('.') == -1))
                Tb_Alt[0].Text = Alttitude.ToString() + ".0";
            else
                Tb_Alt[0].Text = Alttitude.ToString();
            BackgroundDisplay.Children.Add(Tb_Alt[0]);

        }

        //15/3/2016
        //search offline
        //khi nhap toa do thi co the search offline
        //Chi đường chỉ làm được với 1 số địa điểm được đặt tên trên bản đồ như chi đường giữa 2 tỉnh, từ tp hcm
        //đến sân bay nha trang "Sân Bay Nha Trang, Khanh Hoa, Vietnam"
        //san bay tan son nhat "58 Truong Son, Ward 2, Tan Binh District Ho Chi Minh City  Ho Chi Minh City"
        //          endLocation2.Latitude = 10.772099;
        //          endLocation2.Longitude = 106.657693;
        //San bay tan son nhat dLatDentination, dLonDentination
        //san bay da nang 16.044040, 108.199357
        MapLocationFinderResult result_position;
        MapLocation dentination_pos;

        private void Get_Click(object sender, RoutedEventArgs e)
        {
            if (tb_Lat_Search.Text != "" && tb_Lon_Search.Text != "")
            {
                dLatDentination = Convert.ToDouble(tb_Lat_Search.Text);
                dLonDentination = Convert.ToDouble(tb_Lon_Search.Text);
                //Add My home picture
                AddImageAtLatAndLon(dLatDentination, dLonDentination);
            }
        }

        private void Mouse_Click(object sender, TappedRoutedEventArgs e)
        {
            NotifyUser(String.Empty, NotifyType.StatusMessage);
        }

        public async void Search_Offline(string StrDentination)
        {

            //position: string

            try {
                result_position = await MapLocationFinder.FindLocationsAsync(StrDentination, myMap.Center);
                dentination_pos = result_position.Locations.First();
                //show result
                tb_Lat_Search.Text = dentination_pos.Point.Position.Latitude.ToString();
                tb_Lon_Search.Text = dentination_pos.Point.Position.Longitude.ToString();
                //Update Lat and Lon
                dLatDentination = dentination_pos.Point.Position.Latitude;
                dLonDentination = dentination_pos.Point.Position.Longitude;
                //On map
                myMap.Center =
                   new Geopoint(new BasicGeoposition()
                   {
                   //Geopoint for Seattle San Bay Tan Son Nhat:   dLatDentination, dLonDentination

                   Latitude = dentination_pos.Point.Position.Latitude,
                   Longitude = dentination_pos.Point.Position.Longitude
                   });
                myMap.ZoomLevel = 18;
                //Add My home picture
                AddImageAtLatAndLon(dentination_pos.Point.Position.Latitude, dentination_pos.Point.Position.Longitude);

            }
            catch (Exception ex)//bat loi
            {
                tb_Position.Text = ex.Message;
            }
            //if(result.Status == MapLocationFinder.)
            //MapLocation begin = result_position.Locations.First();

            //test show point
            //tb_ZoomLevel.Text = begin.Point.Position.Latitude.ToString() + "  "
            //                    + begin.Point.Position.Longitude.ToString();
            //System.Diagnostics.Debug.WriteLine(routeResult.Status); // DEBUG

        }
        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Add 1 picture at lat, lon
        /// </summary>
        //Ngay 20/03/2016
        Image img_AtLatAndLon = new Image();
        public void AddImageAtLatAndLon(double dLat_Pos, double dLon_Pos)
        {

            myMap.Children.Remove(img_AtLatAndLon);

            //Edit size of image
            img_AtLatAndLon.Height = 5 * myMap.ZoomLevel;
            img_AtLatAndLon.Width = 5 * myMap.ZoomLevel;

            //img_rotate.RenderTransform
            img_AtLatAndLon.Opacity = 0.7;


            //img_rotate.Transitions.
            img_AtLatAndLon.Source = new BitmapImage(new Uri("ms-appx:///Assets/MyHome.png"));

            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            img_AtLatAndLon.RenderTransform = new RotateTransform()
            {

                //Angle = 3.6 * slider.Value,
                //CenterX = 15 * myMap.ZoomLevel / 2,
                //CenterX = 62, //The prop name maybe mistyped 
                //CenterY = 15 * myMap.ZoomLevel / 2 //The prop name maybe mistyped 
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //img_rotate.sca
            img_AtLatAndLon.Stretch = Stretch.Uniform;
            img_AtLatAndLon.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            img_AtLatAndLon.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            img_AtLatAndLon.Margin = new Windows.UI.Xaml.Thickness(-5 * myMap.ZoomLevel / 2, -5 * myMap.ZoomLevel / 2, 0, 0);
            //tbOutputText.Text = "Latitude: " + myMap.Center.Position.Latitude.ToString() + '\n';
            //tbOutputText.Text += "Longitude: " + myMap.Center.Position.Longitude.ToString() + '\n';
            ////tbOutputText.Text += "Timer Fly: " + Timer_fly.ToString();



            Geopoint PointCenterMap = new Geopoint(new BasicGeoposition()
            {
                Latitude = dLat_Pos,
                Longitude = dLon_Pos,
                //Altitude = 200.0
            });
            //myMap.Children.Add(bitmapImage);

            Windows.UI.Xaml.Controls.Maps.MapControl.SetLocation(img_AtLatAndLon, PointCenterMap);
            //myMap.TrySetViewBoundsAsync()
            //Độ dài tương đối của hình so với vị trí mong muốn new Point(0.5, 0.5) không dời
            //Windows.UI.Xaml.Controls.Maps.MapControl.SetNormalizedAnchorPoint(img_rotate, new Point(0.5, 0.5));
            myMap.Children.Add(img_AtLatAndLon);

            //Vẽ quỹ đạo
            //Draw_Trajectory(Convert.ToDouble(Data.Latitude), Convert.ToDouble(Data.Longtitude), Convert.ToDouble(Data.Altitude));

        }

        //Hien toa do khi nhap chuot
        /// <summary>
        /// Used to display messages to the user
        /// </summary>
        /// <param name="strMessage"></param>
        /// <param name="type"></param>
        public void NotifyUser(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    tb_Lat_Search.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    tb_Lon_Search.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    tb_Lat_Search.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    tb_Lon_Search.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }
            tb_Lat_Search.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
        }

        //23/02/2016
        /// <summary>
        /// Add needle when change heading
        /// </summary>
        /// <param name="CenterX"></param>
        /// <param name="CenterY"></param>
        Image Img_Needle = new Image();
        public void AddNeedle(double CenterX, double CenterY)
        {

            //***************************************************************************************
            //9/03/2016 Add thêm ảnh mới
            //Image Img_Needle = new Image();
            BackgroundDisplay.Children.Remove(Img_Needle);
            //Edit size of image
            Img_Needle.Height = 80;
            Img_Needle.Width = 80;

            //Img_Needle.RenderTransform
            Img_Needle.Opacity = 1;


            //Img_Needle.Transitions.
            Img_Needle.Source = new BitmapImage(new Uri("ms-appx:///Assets/Needle.png"));
            //Xoay ảnh
            //kích thước của ảnh là (15 * myMap.ZoomLevel x 15 * myMap.ZoomLevel;);
            //Trung tâm ảnh là (15 * myMap.ZoomLevel / 2) x (15 * myMap.ZoomLevel / 2);
            //khi đặt map ở ở trí lat0, long0 thì chỗ đó là điểm 0, 0 của ảnh
            //Nên để chỉnh tâm ảnh trùng vj trí lat0, long0 thì phỉ dùng margin
            //dời ảnh lên trên 1 nửa chiều dài,
            //dời ảnh sang trái 1 nửa chiều rộng
            Img_Needle.RenderTransform = new RotateTransform()
            {

                //Angle = 0,
                //CenterX = 40,//là la Img_Needle_FliCom.Width/2
                //CenterX = 62, //The prop name maybe mistyped 
                //CenterY = 40 //la Img_Needle_FliCom.Height
            };
            //mặc định ảnh có chiều dài và chiều rộng là vô cùng
            //bitmapImage.PixelHeight
            //Img_Needle.sca
            Img_Needle.Stretch = Stretch.Uniform;
            Img_Needle.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            Img_Needle.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            //Img_Needle.Opacity = 0.8;
            Img_Needle.Margin = new Windows.UI.Xaml.Thickness(-2358 + dConvertToTabletX + CenterX * 2, -798 + dConvertToTabletY + CenterY * 2, 0, 0);
            BackgroundDisplay.Children.Add(Img_Needle);

        }
        //////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// dHeading: góc quay của bản đồ
        /// </summary>
        /// <param name="dAngle"></param>
        public void Rotate_Needle(double dHeading)
        {

            BackgroundDisplay.Children.Remove(Img_Needle);
            //center width/2, height/2
            Img_Needle.RenderTransform = new RotateTransform()
            {

                Angle = 360 - dHeading,
                CenterX = 40,
                //CenterX = 62, //The prop name maybe mistyped 
                CenterY = 40
            };

            BackgroundDisplay.Children.Add(Img_Needle);


        }
    }
    //////////////////////////////////////////////////////////////////////////////////////////
    public enum NotifyType
    {
        StatusMessage,
        ErrorMessage
    };


    //Ngày 27/02/2016
    //**********************************************************************
    /*
    Viết theo cấu trúc software artchitecture
    */

    /// <summary>
    /// Quản lý tất cả các phần tử, các hàm trong dự án, layer cao nhất
    /// Có nhiệm vụ giao tiếp với bên ngoài
    /// App --> Display --> Setup, Run --> UART --> Microcontroller --> Sensor
    /// </summary>
    /// 
    public class App_UserInterface
    {

        //MapControl mymap = new MapControl();
        /// <summary>
        /// Màn hình giao diện chính gồm phần connect sau đó là hiển thị
        /// </summary>
        public class Display
        {
            /// <summary>
            /// Hiệu chỉnh vị trí các Button và textblock
            /// Vẽ ra các  bộ phận hiển thị thông số máy bay
            /// thiết lập dữ liệu ban đầu là 0
            /// </summary>
            public class Setup
            {
                /// <summary>
                /// Setup hiện map lên giao diện chính
                /// </summary>
                public class MapOffline
                {
                    public MapControl MC_Offine = new MapControl();
                    /// <summary>
                    /// Set vị trí cho map
                    /// </summary>
                    public void SetupPosition()
                    {

                        MC_Offine.HorizontalAlignment = HorizontalAlignment.Left;
                        MC_Offine.VerticalAlignment = VerticalAlignment.Top;
                        MC_Offine.Margin = new Thickness(0, 0, -12, 0);
                        MC_Offine.Height = 730;
                        MC_Offine.Width = 1363;
                        MC_Offine.Loaded += MCOffline_Loaded;
                    }

                    public void MCOffline_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
                    {
                        MC_Offine.Center =
                            new Geopoint(new BasicGeoposition()
                            {
                                //Geopoint for Seattle San Bay Tan Son Nhat:   dLatDentination, dLonDentination
                                //    Latitude = dLatDentination,
                                //Longitude = dLonDentination
                            });
                        MC_Offine.ZoomLevel = 12;
                        MC_Offine.Style = MapStyle.Road;
                    }
                    //****************************************************************************************************


                }

                /// <summary>
                /// Setup timer để đọc và hiển thị dữ liệu
                /// </summary>
                public class Timer
                {

                }

                /// <summary>
                /// Tạo màn hình giao diện với đầy đủ thông số theo yêu cầu của khách hàng
                /// </summary>
                public class Screen
                {

                }

            }
            //**********************************************************


            /// <summary>
            /// Chứa các timer để update data
            /// </summary>
            public class Run
            {
                /// <summary>
                /// Xử lý data mà cổm com nhận được
                /// </summary>
                public class Process_data
                {
                    /// <summary>
                    /// Đọc data liên tục từ cổng com
                    /// </summary>
                    public class Get_data
                    {

                    }

                    /// <summary>
                    /// xử lý data đọc được và đưa và các biến toàn cục
                    /// </summary>
                    public class Translate_data
                    {

                    }
                }

                /// <summary>
                /// Update dữ liệu liên tục lên các bộ phận hiển thị
                /// </summary>
                public class UpdateDisplay
                {
                    /// <summary>
                    /// Update tất cả thông số Speed, độ cao, heading, Roll, Pitch
                    /// Góc đến đích và khảng cách
                    /// </summary>
                    public class UpdateParameter
                    {

                    }

                    /// <summary>
                    /// Vẽ quỹ đạo 
                    /// </summary>
                    public class DrawTrajectory
                    {

                    }

                }


            }

        }

        /// <summary>
        /// Màn hình để bảo mật
        /// Yêu cầu user phải đăng nhập
        /// </summary>
        public class Login
        {

        }

    }

    //Ngày 27/02/2016
    //****************************************************
    //****************************************************
    /*
    Viết theo cấu trúc root --> Task --> tag
    */
    /// <summary>
    /// Tag_low gồm Get Data, Translate Data, Update Parameter, Draw Trajectory
    /// </summary>
    public class Tag_low
    {
        public string Name;
        public string Quality;
        public DateTime TimeStamp;
        int Value;
        public Task Parent;

        public Tag_low(string name)
        {
            Name = name;
        }

        public int GetValue()
        {
            return Value;
        }

        public void SetValue(int Value)
        {
            this.Value = Value;
        }
    }

    /// <summary>
    /// Tag_high của Setup gồm Map Offline, Timer, Screen
    /// Tag_high của Run gồm Process Data và Update Display
    /// </summary>
    public class Tag_high
    {
        public string Name;
        public string Quality;
        public DateTime TimeStamp;
        int Value;
        public Task Parent;

        public Tag_high(string name)
        {
            Name = name;
        }

        public int GetValue()
        {
            return Value;
        }

        public void SetValue(int Value)
        {
            this.Value = Value;
        }
    }
    //*************************************************************************
    /// <summary>
    /// Gồm: Set up, Run
    /// </summary>
    public class Task_
    {
        public string Name;
        public uint Period;
        public ArrayList Tags = null;
        //Timer timer = null;
        public Root Parent;

        public Task_(string name, uint period)
        {
            Name = name;
            Period = period;
            Tags = new ArrayList();//quan ly tag
        }

        public void AddTag(Tag_high tag)
        {
            //tag.Parent = this;
            Tags.Add(tag);
        }

        public Tag_high FindTag(string nametag)
        {
            Tag_high tag = null;
            for (int i = 0; i < Tags.Count; i++)
            {
                tag = (Tag_high)Tags[i];
                if (tag.Name == nametag)
                {
                    return tag;
                }
            }
            return null;
        }
        public void Setup_MapOffline()
        {

        }
    }

    /// <summary>
    /// Root là Display
    /// </summary>
    public class Root
    {
        ArrayList Tasks = new ArrayList();
        //Tag_high
        public void AddTask(Task_ task)
        {
            task.Parent = this;
            Tasks.Add(task);
        }

        public Task_ FindTask(string nametask)
        {
            Task_ task = null;
            for (int i = 0; i < Tasks.Count; i++)
            {
                task = (Task_)Tasks[i];
                if (task.Name == nametask)
                    return task;
            }
            return null;
        }

        public void RunTask(string nametask)
        {
            Task_ task = null;
            task.Setup_MapOffline();
        }

    }



}