using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhaoxi.DigitaPlatform.Entities;

namespace DigitalPlatform.IDAL
{
    public interface ILocalDataAccess
    {
        DataTable Login(string username, string password);

        void SaveDevice(List<DeviceEntity> devices);
        List<DeviceEntity> GetDevices();

        List<ThumbEntity> GetThumbList();
        public int SaveAlarmMessage(AlarmEntity alarm);
        List<PropEntity> GetPropertyOption();
        void SaveTrend(List<TrendEntity> trends);
        List<TrendEntity> GetTrends();
        List<AlarmEntity> GetAlarmList(string condition);

        int UpdateAlarmState(string aNum,string solveTime);

        void SaveRecord(List<RecordWriteEntity> records);
        List<RecordReadEntity> GetRecords();
        void ResetPassword(string username);
        void GetBaseInfo(List<BaseInfoEntity> baseInfos, List<UserEntity> users);
        void SaveBaseInfo(List<BaseInfoEntity> baseInfo, List<UserEntity> users);

    }
}
