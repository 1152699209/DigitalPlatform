using DigitalPlatform.DataAccess;
using DigitalPlatform.IDAL;
using DigitalPlatform.Models;
using Entity;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.ViewModels
{
    public class SettingsViewMdole : ViewModelBase
    {
        #region 常规设置
        private string _systemName;

        public string SystemName
        {
            get { return _systemName; }
            set { Set(ref _systemName, value); }
        }
        private string _dataBufferSize;

        public string DataBufferSize
        {
            get { return _dataBufferSize; }
            set { Set(ref _dataBufferSize, value); }
        }
        private string _logBufferSize;

        public string LogBufferSize
        {
            get { return _logBufferSize; }
            set { Set(ref _logBufferSize, value); }
        }
        private string _logPath;

        public string LogPath
        {
            get { return _logPath; }
            set { Set(ref _logPath, value); }
        }
        #endregion

        // 监测配置
        public List<SettingsInfoModel> MonitorList { get; set; } =
            new List<SettingsInfoModel>();
        public List<DeviceItemModel> DeviceList { get; set; }

        // 用户管理
        public ObservableCollection<UserModel> Users { get; set; } =
            new ObservableCollection<UserModel>();
        public List<UserTypeModel> UserTypeList { get; set; } =
            new List<UserTypeModel>()
            {
                new UserTypeModel(0,"操作员"),
                new UserTypeModel(1,"技术员"),
                new UserTypeModel(10,"信息管理员"),
            };


        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }

        public RelayCommand AddUserCommand { get; set; }
        public RelayCommand<UserModel> DelUserCommand { get; set; }
        public RelayCommand<UserModel> ResetPwdCommand { get; set; }

        public RelayCommand ExportCommand { get; set; }
        public RelayCommand InportCommand { get; }

        ILocalDataAccess _localDataAccess;
        public SettingsViewMdole(ILocalDataAccess localDataAccess, MainViewModel mainViewModel)
        {
            _localDataAccess = localDataAccess;
            DeviceList = mainViewModel.DeviceList.Where(d => d.VariableList.Count > 0).ToList();

            Refresh();

            RefreshCommand = new RelayCommand(Refresh);

            AddUserCommand = new RelayCommand(AddUser);
            DelUserCommand = new RelayCommand<UserModel>(DeleteUser);
            ResetPwdCommand = new RelayCommand<UserModel>(ResetPassword);

            ExportCommand = new RelayCommand(Export);
            InportCommand = new RelayCommand(Inport);

            SaveCommand = new RelayCommand(Save);

        }
        private void Refresh()
        {
            List<BaseInfoEntity> baseInfos = new List<BaseInfoEntity>();
            List<UserEntity> users = new List<UserEntity>();
            _localDataAccess.GetBaseInfo(baseInfos, users);

            Users = new ObservableCollection<UserModel>(users.Select(u => new UserModel
            {
                UserName = u.UserName,
                Password = u.Password,
                UserType = int.Parse(u.UserType),
                RealName = u.RealName,
                Gender = int.Parse(u.Gender),
                PhoneNumber = u.PhoneNum,
                Department = u.Department
            }).ToList());

            InitBaseInfo(baseInfos);
        }
        private void InitBaseInfo(List<BaseInfoEntity> baseInfos)
        {
            SystemName = baseInfos.FirstOrDefault(b => b.BaseNum == "B001").Value;
            DataBufferSize = baseInfos.FirstOrDefault(b => b.BaseNum == "B002").Value;
            LogBufferSize = baseInfos.FirstOrDefault(b => b.BaseNum == "B003").Value;
            LogPath = baseInfos.FirstOrDefault(b => b.BaseNum == "B004").Value;


            MonitorList.Clear();
            foreach (var item in baseInfos.Where(b => b.type == "2"))
            {
                MonitorList.Add(new SettingsInfoModel
                {
                    BaseNum = item.BaseNum,
                    Header = item.Header,
                    Desc = item.Description,
                    DeviceList = DeviceList,
                    DeviceNum = item.DeviceNum,
                    VariableNum = item.VariableNum
                });
            }
            this.RaisePropertyChanged(nameof(MonitorList));
        }


        private void AddUser() => Users.Add(new UserModel());
        private void DeleteUser(UserModel model) => Users.Remove(model);
        private void ResetPassword(UserModel model) => _localDataAccess.ResetPassword(model.UserName);

        private List<BaseInfoEntity> GetBaseInfoList()
        {
            List<BaseInfoEntity> baseInfoEntities = new List<BaseInfoEntity>();
            baseInfoEntities.Add(new BaseInfoEntity
            {
                BaseNum = "B001",
                type = "1",
                Value = this.SystemName
            });
            baseInfoEntities.Add(new BaseInfoEntity
            {
                BaseNum = "B002",
                type = "1",
                Value = this.DataBufferSize
            });
            baseInfoEntities.Add(new BaseInfoEntity
            {
                BaseNum = "B003",
                type = "1",
                Value = this.LogBufferSize
            });
            baseInfoEntities.Add(new BaseInfoEntity
            {
                BaseNum = "B004",
                type = "1",
                Value = this.LogPath
            });

            foreach (var item in this.MonitorList)
            {
                baseInfoEntities.Add(new BaseInfoEntity
                {
                    BaseNum = item.BaseNum,
                    Header = item.Header,
                    Description = item.Desc,
                    type = "2",
                    DeviceNum = item.DeviceNum,
                    VariableNum = item.VariableNum,
                });
            }

            return baseInfoEntities;
        }
        private void Export()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "配置" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".cfg";
            if (saveFileDialog.ShowDialog() == true)
            {
                string json = System.Text.Json.JsonSerializer.Serialize(this.GetBaseInfoList());
                System.IO.File.WriteAllText(saveFileDialog.FileName, json);
            }
        }
        private void Inport()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string json = System.IO.File.ReadAllText(openFileDialog.FileName);
                var baseInfo = System.Text.Json.JsonSerializer.Deserialize<List<BaseInfoEntity>>(json);
                this.InitBaseInfo(baseInfo);
            }
        }

        private void Save()
        {
            List<BaseInfoEntity> baseInfoEntities = this.GetBaseInfoList();

            List<UserEntity> users = Users.Select(u => new UserEntity
            {
                UserName = u.UserName,
                Password = u.Password,
                UserType = u.UserType.ToString(),
                RealName = u.RealName,
                Gender = u.Gender.ToString(),
                PhoneNum = u.PhoneNumber,
                Department = u.Department,
            }).ToList();
            _localDataAccess.SaveBaseInfo(baseInfoEntities, users);
        }
    }
}
