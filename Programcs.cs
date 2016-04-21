using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoAn
{
    class Programcs
    {
    }

    /// <summary>
    /// Quản lý tất cả các phần tử, các hàm trong dự án, layer cao nhất
    /// Có nhiệm vụ giao tiếp với bên ngoài
    /// App --> Display --> Setup, Run --> UART --> Microcontroller --> Sensor
    /// </summary>
    public class App_UserInterface
    {

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
}
