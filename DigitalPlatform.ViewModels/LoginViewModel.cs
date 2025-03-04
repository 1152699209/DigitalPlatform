using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows;
using DigitalPlatform.Models;
using DigitalPlatform.IDAL;
using GalaSoft.MvvmLight.Ioc;

namespace DigitalPlatform.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {

        public UserModel User { get; set; }
        private ILocalDataAccess _localDataAccess;
        public RelayCommand<object> LoginCommand { get; set; }

        //错误提示信息
        public string _failedMsg;
        public string FailedMsg
        {
            get { return _failedMsg; }
            set { Set(ref _failedMsg, value); }
        }


        public LoginViewModel(ILocalDataAccess localDataAccess)
        {
            if (!IsInDesignMode)
            {
                User = new UserModel();
                LoginCommand = new RelayCommand<object>(DoLogin);
            }
            _localDataAccess = localDataAccess;
        }
        private void DoLogin(object obj)
        {
            try
            {
                var data = _localDataAccess.Login(User.UserName, User.Password);
                // 对接数据库
                if (data == null)
                {
                    throw new Exception("登陆失败,没有用户信息");
                }

                var main = SimpleIoc.Default.GetInstance<MainViewModel>();
                if (main != null)
                {
                    main.GlobalUserInfo.UserName = User.UserName;
                    main.GlobalUserInfo.Password = User.Password;
                    main.GlobalUserInfo.RealName = data.Rows[0]["real_name"].ToString()!;
                    main.GlobalUserInfo.UserType = int.Parse(data.Rows[0]["user_type"].ToString()!);
                    main.GlobalUserInfo.Gender = int.Parse(data.Rows[0]["gender"].ToString()!);
                    main.GlobalUserInfo.Department = data.Rows[0]["department"].ToString()!;
                    main.GlobalUserInfo.PhoneNumber = data.Rows[0]["phone_num"].ToString()!;
                }

                (obj as Window).DialogResult = true;
            }

            catch (Exception ex)
            {
                FailedMsg = ex.Message;
            }

        }
    }
}
