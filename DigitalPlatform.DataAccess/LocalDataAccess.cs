using DigitalPlatform.Entities;
using DigitalPlatform.IDAL;
using Entity;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using Zhaoxi.DigitaPlatform.Entities;

namespace DigitalPlatform.DataAccess
{
    public class LocalDataAccess : ILocalDataAccess
    {
        #region 基础方法
        // Sqlite的数据处理对象
        private SQLiteConnection conn = null;// 建立连接
        private SQLiteCommand comm = null;// 执行SQL
        private SQLiteDataAdapter adapter = null;// 填充数据列表
        private SQLiteTransaction trans = null;//事务

        // Sqlite数据库的连接字符串
        string connStr = "Data Source=data.db;Version=3;";
        /// <summary>
        /// 执行一个SQL语句，并将结果通过DataTable返回
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private DataTable GetDatas(string sql, Dictionary<string, object> param = null)
        {
            try
            {
                if (conn == null)
                    conn = new SQLiteConnection(connStr);
                conn.Open();

                adapter = new SQLiteDataAdapter(sql, conn);

                if (param != null)
                {
                    List<SQLiteParameter> parameters = new List<SQLiteParameter>();
                    foreach (var item in param)
                    {
                        parameters.Add(
                            new SQLiteParameter(item.Key, DbType.String)
                            { Value = item.Value }
                        );
                        adapter.SelectCommand.Parameters.Add(new SQLiteParameter(item.Key, DbType.String)
                        { Value = item.Value });
                    }
                    //adapter.SelectCommand.Parameters.AddRange(parameters.ToArray());
                }
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                return dataTable;
            }
            catch (Exception ex) { throw ex; }
            finally
            {
                this.Dispose();
            }
        }
        public int SaveAlarmMessage(AlarmEntity alarm)
        {
            string sql = "insert into alarms(a_num,c_num,d_num,v_num,content,date_time,level,state,alarm_value,user_id)" +
                $"values('{alarm.AlarmNum}','{alarm.CNum}','{alarm.DeviceNum}','{alarm.VariableNum}','{alarm.AlarmContent}','{alarm.RecordTime}','{alarm.AlarmLevel}','{alarm.State}','{alarm.RecordValue}','{alarm.UserId}')";
            return this.SqlExecute(new List<string> { sql });
        }
        public List<PropEntity> GetPropertyOption()
        {
            string sql = "select * from properties";
            DataTable dt1 = this.GetDatas(sql, null);

            var result = (from q1 in dt1.AsEnumerable()
                          select new PropEntity
                          {
                              PropHeader = q1["p_header"].ToString(),
                              PropName = q1["p_name"].ToString(),
                              PropType = int.Parse(q1["p_type"].ToString())
                          }).ToList();

            return result;
        }
        private int SqlExecute(List<string> sqls)
        {
            int count = 0;
            try
            {
                if (conn == null)
                    conn = new SQLiteConnection(connStr);
                conn.Open();

                trans = conn.BeginTransaction();
                foreach (var sql in sqls)
                {
                    comm = new SQLiteCommand(sql, conn);
                    count += comm.ExecuteNonQuery();
                }
                trans.Commit();
                return count;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                this.Dispose();
            }

        }
        private void Dispose()
        {
            if (trans != null)
            {
                trans.Dispose();
                trans = null;
            }
            if (adapter != null)
            {
                adapter.Dispose();
                adapter = null;
            }
            if (comm != null)
            {
                comm.Dispose();
                comm = null;
            }
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
                conn = null;
            }
        }
        #endregion
        #region 登录逻辑
        public DataTable Login(string username, string password)
        {
            // 不能拼接 ，Sql注入攻击
            string userSql = "select * from sys_users where user_name=@user_name and password=@password";

            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("@user_name", username);
            param.Add("@password", password);

            DataTable dataTable = this.GetDatas(userSql, param);
            if (dataTable.Rows.Count == 0)
                throw new Exception("用户名或密码错误");

            return dataTable;
        }


        #endregion


        public void SaveDevice(List<DeviceEntity> devices)
        {
            try
            {
                int count = 0;
                conn = new SQLiteConnection(connStr);
                conn.Open();
                //开启事务
                trans = conn.BeginTransaction();

                string sql = "delete from devices;" +
                    "delete from device_properties;" +
                    "delete from variables;" +
                    "delete from conditions;" +
                    "delete from manual_controls;" +
                     "delete from union_devices;";
                comm = new SQLiteCommand(sql, conn);
                comm.ExecuteNonQuery();//删除

                foreach (var item in devices)
                {
                    sql = $"insert into devices(d_num,x,y,z,w,h,d_type_name,header,flow_direction,rotate) " +
                        $"values('{item.DeviceNum}','{item.X}','{item.Y}','{item.Z}','{item.W}','{item.H}','{item.DeviceTypeName}','{item.Header}','{item.FlowDirection}','{item.Rotate}')";
                    comm.CommandText = sql;
                    var flag = comm.ExecuteNonQuery();// 插入
                    count += flag;

                    foreach (var p in item.Props)
                    {
                        //名字或值有一个为空 就是不合法的
                        if (string.IsNullOrEmpty(p.PropName) || string.IsNullOrEmpty(p.PropValue)) continue;

                        sql = $"insert into device_properties(d_num,prop_name,prop_value) values('{item.DeviceNum}','{p.PropName}','{p.PropValue}')";
                        comm.CommandText = sql;
                        comm.ExecuteNonQuery();// 插入
                    }
                    // 保存对应的点位信息
                    // 清除点位前,需要将报警条件\联控条件清除,再重新添加

                    foreach (var v in item.Vars)
                    {
                        if (string.IsNullOrEmpty(v.Header) || string.IsNullOrEmpty(v.Address)) continue;

                        sql = $"insert into variables(d_num,v_num,header,address,offset,modulus,v_type) " +
                            $"values('{item.DeviceNum}','{v.VarNum}','{v.Header}','{v.Address}','{v.Offset}','{v.Modulus}','{v.VarType}')";
                        comm.CommandText = sql;
                        comm.ExecuteNonQuery();// 插入

                        foreach (var c in v.AlarmConditions)
                        {
                            if (string.IsNullOrEmpty(c.Operator) || string.IsNullOrEmpty(c.CompareValue)) continue;

                            sql = "insert into conditions(v_num,c_num,operator,value,message,c_type) " +
                                $"values('{v.VarNum}','{c.CNum}','{c.Operator}','{c.CompareValue}','{c.AlarmContent}',1)";
                            comm.CommandText = sql;
                            comm.ExecuteNonQuery();// 插入

                            //comm.Parameters
                        }

                        // 保存手动控制列表
                        foreach (var m in item.ManualControls)
                        {
                            if (string.IsNullOrEmpty(m.Address) || string.IsNullOrEmpty(m.Value)) continue;

                            sql = "insert into manual_controls(d_num,c_header,c_address,c_value)" +
                                $" values('{item.DeviceNum}','{m.Header}','{m.Address}','{m.Value}')";
                            comm.CommandText = sql;
                            comm.ExecuteNonQuery();// 插入
                        }

                        // 保存联控条件
                        foreach (var c in v.UnionConditions)
                        {
                            if (string.IsNullOrEmpty(c.Operator) || string.IsNullOrEmpty(c.CompareValue)) continue;

                            sql = $"insert into conditions(v_num,c_num,operator,value,message,c_type)" +
                                $" values('{v.VarNum}','{c.CNum}','{c.Operator}','{c.CompareValue}','{c.AlarmContent}',2)";
                            comm.CommandText = sql;
                            comm.ExecuteNonQuery();// 插入

                            // 保存联控设备信息
                            foreach (var ud in c.UnionDevices)
                            {
                                if (string.IsNullOrEmpty(ud.DNum) ||
                                    string.IsNullOrEmpty(ud.VAddr) ||
                                    string.IsNullOrEmpty(ud.Value) ||
                                    string.IsNullOrEmpty(ud.VType)) continue;

                                sql = $"insert into union_devices(c_num,u_num,d_num,v_addr,value,v_type)" +
                                    $" values('{c.CNum}','{ud.UNum}','{ud.DNum}','{ud.VAddr}','{ud.Value}','{ud.VType}')";
                                comm.CommandText = sql;
                                comm.ExecuteNonQuery();// 插入
                            }
                        }


                    }
                    //对应的报警信息 同步删除
                }
                //全部完成后提交事务
                if (count != devices.Count)
                {
                    throw new Exception("设备数据未完全保存");
                }
                trans.Commit();
            }
            catch (Exception ex)
            {
                //发生异常就回滚
                trans.Rollback();
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
        }
        public List<DeviceEntity> GetDevices()
        {

            string sql = "select * from devices";
            DataTable dt_device = this.GetDatas(sql, null);

            sql = $"select * from device_properties";
            DataTable dt_props = this.GetDatas(sql, null);

            sql = $"select * from variables";
            DataTable dt_vars = this.GetDatas(sql, null);

            sql = $"select * from manual_controls";
            DataTable dt_manual = this.GetDatas(sql, null);


            sql = $"select * from conditions";
            DataTable dt_conditions = this.GetDatas(sql, null);

            sql = $"select * from union_devices";
            DataTable dt_union = this.GetDatas(sql, null);

            var result = dt_device.AsEnumerable().Select(d =>
            {
                return new DeviceEntity()
                {
                    DeviceNum = d["d_num"].ToString(),
                    Header = d["header"].ToString(),
                    X = d["x"].ToString(),
                    Y = d["y"].ToString(),
                    Z = d["z"].ToString(),
                    W = d["w"].ToString(),
                    H = d["h"].ToString(),
                    DeviceTypeName = d["d_type_name"].ToString(),
                    FlowDirection = d["flow_direction"].ToString(),
                    Rotate = d["rotate"].ToString(),
                    Props = dt_props.AsEnumerable().Where(dp => dp["d_num"].ToString() == d["d_num"].ToString()).Select
                    (dp => new Entities.DevicePropItemEntity()
                    {
                        PropName = dp["prop_name"].ToString(),
                        PropValue = dp["prop_value"].ToString(),
                    }).ToList(),

                    Vars = dt_vars.AsEnumerable().Where(dp => dp["d_num"].ToString() == d["d_num"].ToString()).Select
                    (dv => new VariableEntity()
                    {
                        VarNum = dv["v_num"].ToString(),
                        Header = dv["header"].ToString(),
                        Address = dv["address"].ToString(),
                        Offset = double.Parse(dv["offset"].ToString()),
                        Modulus = double.Parse(dv["modulus"].ToString()),
                        VarType = dv["v_type"].ToString(),
                        AlarmConditions = dt_conditions.AsEnumerable()
                        .Where(dc => dc["v_num"].ToString() == dv["v_num"].ToString() && dc["c_type"].ToString() == "1")
                        .Select(dc => new ConditionEntity()
                        {
                            CNum = dc["c_num"].ToString(),
                            Operator = dc["operator"].ToString(),
                            CompareValue = dc["value"].ToString(),
                            AlarmContent = dc["message"].ToString()
                        }).ToList(),
                        UnionConditions = dt_conditions.AsEnumerable()
                        .Where(dc => dc["v_num"].ToString() == dv["v_num"].ToString() && dc["c_type"].ToString() == "2")
                        .Select(dc => new ConditionEntity
                        {
                            CNum = dc["c_num"].ToString(),
                            Operator = dc["operator"].ToString(),
                            CompareValue = dc["value"].ToString(),
                            UnionDevices = dt_union.AsEnumerable()
                            .Where(du => du["c_num"].ToString() == dc["c_num"].ToString())
                            .Select(du => new UDevuceEntity
                            {
                                UNum = du["u_num"].ToString(),
                                DNum = du["d_num"].ToString(),
                                VAddr = du["v_addr"].ToString(),
                                Value = du["value"].ToString(),
                                VType = du["v_type"].ToString()
                            }).ToList()
                        }).ToList()


                    }).ToList(),
                    ManualControls = dt_manual.AsEnumerable().Where(dm => dm["d_num"].ToString() == d["d_num"].ToString()).Select(dm =>
                    new ManualEntity()
                    {
                        Header = dm["c_header"].ToString(),
                        Address = dm["c_address"].ToString(),
                        Value = dm["c_value"].ToString(),
                    }).ToList(),


                };
            });
            return result.ToList();
        }

        public List<ThumbEntity> GetThumbList()
        {
            string sql = "select * from thumbs";
            DataTable dt = this.GetDatas(sql, null);

            var result = from d in dt.AsEnumerable()
                         select new ThumbEntity
                         {
                             Icon = d["icon"].ToString(),
                             Header = d["header"].ToString(),
                             TargetType = d["target_type"].ToString(),
                             Width = int.Parse(d["w"].ToString()),
                             Height = int.Parse(d["h"].ToString()),
                             Category = d["category"].ToString()
                         };

            return result.ToList();
        }

        #region 保存趋势图表配置信息
        public void SaveTrend(List<TrendEntity> trends)
        {
            try
            {
                int count = 0;
                conn = new SQLiteConnection(connStr);
                conn.Open();
                trans = conn.BeginTransaction();

                string sql =
                    "delete from trend;" +
                    "delete from axis;" +
                    "delete from section;" +
                    "delete from series;";

                comm = new SQLiteCommand(sql, conn);
                comm.ExecuteNonQuery();//删除

                foreach (var item in trends)
                {
                    sql = "insert into trend(t_num,trend_header,show_legend) " +
                        $"values('{item.TNum}','{item.THeader}','{item.ShowLegend}')";
                    comm.CommandText = sql;
                    var flag = comm.ExecuteNonQuery();// 插入
                    count += flag;

                    // 保存对应的属性
                    // 属性

                    //添加轴信息
                    foreach (var a in item.Axes)
                    {
                        sql = "insert into axis(trend_num,axis_num,title,show_title,min,max,show_seperator,label_formatter,position) " +
                            $"values('{item.TNum}','{a.ANum}','{a.Title}','{a.IsShowTitle.ToString()}','{a.Minimum}','{a.Maximum}','{a.IsShowSeperator.ToString()}','{a.LabelFormatter}','{a.Position}')";
                        comm.CommandText = sql;
                        comm.ExecuteNonQuery();// 插入

                        foreach (var s in a.Sections)
                        {
                            sql = "insert into section(axis_num,value,color) " +
                                $"values('{a.ANum}','{s.Value}','{s.Color}')";
                            comm.CommandText = sql;
                            comm.ExecuteNonQuery();// 插入
                        }
                    }
                    // 保存对应的点位信息
                    // 清除点位前,需要将报警条件\联控条件清除,再重新添加
                    foreach (var s in item.Series)
                    {
                        sql = "insert into series(d_num,v_num,t_num,title,color,axis_index)" +
                            $" values('{s.DNum}','{s.VNum}','{item.TNum}','{s.Title}','{s.Color}','{s.AxisNum}')";
                        comm.CommandText = sql;
                        comm.ExecuteNonQuery();// 插入
                    }
                }
                if (count != trends.Count)
                    throw new Exception("设备数据未完全保存");

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.Rollback();
                throw ex;
            }
            finally
            {
                this.Dispose();
            }
        }

        public List<TrendEntity> GetTrends()
        {
            string sql = "select * from trend";
            DataTable dt_trend = this.GetDatas(sql, null);

            sql = $"select * from axis";
            DataTable dt_axis = this.GetDatas(sql, null);

            sql = $"select * from section";
            DataTable dt_section = this.GetDatas(sql, null);

            sql = $"select * from series";
            DataTable dt_series = this.GetDatas(sql, null);

            var result = dt_trend.AsEnumerable().Select(dt =>
            {
                return new TrendEntity()
                {
                    TNum = dt["t_num"].ToString(),
                    THeader = dt["trend_header"].ToString(),
                    ShowLegend = bool.Parse(dt["show_legend"].ToString()),

                    Axes = dt_axis.AsEnumerable().Where(da => da["trend_num"].ToString() == dt["t_num"].ToString())
                                .Select(da => new AxisEntity()
                                {
                                    ANum = da["axis_num"].ToString(),
                                    Title = da["title"].ToString(),
                                    IsShowTitle = bool.Parse(da["show_title"].ToString()),
                                    Minimum = da["min"].ToString(),
                                    Maximum = da["max"].ToString(),
                                    IsShowSeperator = bool.Parse(da["show_seperator"].ToString()),
                                    LabelFormatter = da["label_formatter"].ToString(),
                                    Position = da["position"].ToString(),
                                    Sections = dt_section.AsEnumerable().Where(ds => ds["axis_num"].ToString() == da["axis_num"].ToString())
                                        .Select(ds => new SectionEntity()
                                        {
                                            Value = ds["value"].ToString(),
                                            Color = ds["color"].ToString()
                                        }).ToList()
                                }).ToList(),
                    Series = dt_series.AsEnumerable().Where(ds => ds["t_num"].ToString() == dt["t_num"].ToString())
                                .Select(ds => new SeriesEntity()
                                {
                                    DNum = ds["d_num"].ToString(),
                                    VNum = ds["v_num"].ToString(),
                                    Title = ds["title"].ToString(),
                                    Color = ds["color"].ToString(),
                                    AxisNum = ds["axis_index"].ToString()
                                }).ToList()
                };
            });

            return result.ToList();
        }
        #endregion

        public List<AlarmEntity> GetAlarmList(string condition)
        {
            //加个1=1 实现条件拼接 这样当condition为空 不需要特殊处理
            string sql = "select a.*,b.header d_header,d.header v_header,e.real_name from alarms a " +
               "LEFT JOIN devices b on b.d_num=a.d_num " +
               "LEFT JOIN variables d on d.v_num=a.v_num " +
               "LEFT JOIN sys_users e ON e.user_name=a.user_id " +
               "where 1=1 ";

            //做一个模糊查询                               
            if (!string.IsNullOrEmpty(condition))
            {
                sql += $" and (a.d_num LIKE '%{condition}%' or" +
                    $" b.header LIKE '%{condition}%' or" +
                    $" a.v_num LIKE '%{condition}%' or" +
                    $" d.header LIKE '%{condition}%')";
            }
            //按照报警时间排序
            sql += "order by a.date_time desc";
            return this.GetDatas<AlarmEntity>(sql, null);
        }

        // ORM中的处理
        private List<T> GetDatas<T>(string sql, Dictionary<string, object> param = null)
        {
            //创建对应类型的列表
            List<T> dataList = new List<T>();
            var dt = GetDatas(sql, param);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //获取当前类型的属性列表
                var pis = typeof(T).GetProperties();
                //创建一个当前类型的实例
                var item = (T)Activator.CreateInstance(typeof(T));
                foreach (var pi in pis)
                {
                    //获取属性名
                    var columnName = pi.Name;
                    //当前属性有列特性
                    if (pi.IsDefined(typeof(ColumnAttribute)))
                    {
                        //获取当前定义的特性名字
                        var att = (ColumnAttribute)pi.GetCustomAttribute(typeof(ColumnAttribute));
                        if (att != null)
                            columnName = att.Name;
                    }
                    if (dt.Columns.Contains(columnName))
                    {
                        pi.SetValue(item, dt.Rows[i][columnName].ToString());
                    }
                }
                dataList.Add(item);
            }
            return dataList;
        }

        public int UpdateAlarmState(string aNum, string solveTime)
        {
          string sql = $"update alarms set state = '1',solve_time='{solveTime}' where a_num = '{aNum}'";
          return this.SqlExecute(new List<string> { sql });
        }

        public List<RecordReadEntity> GetRecords()
        {
            string sql = "SELECT " +
                "var_name, " +
                "var_num, " +
                "device_num," +
                "device_name," +
                "record_value as last_value, " +//记录值最新值
                "AVG( record_value ) AS avg, " +//记录值的最大值
                "Max( record_value ) AS max, " +//记录值的最大值
                "min( record_value ) AS min, " +//记录值的最小值
                //报警次数
                //并做条件判断 当alarm_num 为空或null就返回0  否则范围1
                "SUM( CASE WHEN alarm_num = '' OR alarm_num ISNULL THEN 0 ELSE 1 END ) alarm_count, " +
                //联控报警次数
                "SUM( CASE WHEN union_num = '' OR union_num ISNULL THEN 0 ELSE 1 END ) union_count , " +
                //首先把字符串格式的事件 转为时间戳，取一个最大值
                "datetime(MAX(strftime('%s',record_time)),'unixepoch') last_time, " +
                //计算次数
                "count(1) record_count " +
                "FROM Record " +
                //按照设备编号和参数编号分组
                "GROUP BY device_num,var_num";
            return this.GetDatas<RecordReadEntity>(sql, null);
        }

        public void SaveRecord(List<RecordWriteEntity> records)
        {
            List<string> sqls = new List<string>();
            foreach (var item in records)
            {
                string sql = "insert into record(device_num,device_name,var_num,var_name,record_value,alarm_num,union_num,record_time,user_name)" +
                    $" values('{item.DeviceNum}','{item.DeviceName}','{item.VarNum}','{item.VarName}','{item.RecordValue}','{item.AlarmNum}','{item.UnionNum}','{item.RecordTime}','{item.UserName}')";
                sqls.Add(sql);
            }

            this.SqlExecute(sqls);
        }


        public void GetBaseInfo(List<BaseInfoEntity> baseInfos, List<UserEntity> users)
        {
            //把获取配置信息 和用户数据放在一起
            if (baseInfos != null)
            {
                string sql = "select * from base_info";
                this.GetDatas<BaseInfoEntity>(sql, null).ForEach(i => baseInfos.Add(i));
            }

            if (users != null)
            {
                string sql = "select * from sys_users";
                this.GetDatas<UserEntity>(sql, null).ForEach(i => users.Add(i));
            }
        }

        public void SaveBaseInfo(List<BaseInfoEntity> baseInfo, List<UserEntity> users)
        {
            List<string> sqls = new List<string>()
            { "delete from base_info;"};

            foreach (var item in baseInfo)
            {
                string sql = "insert into base_info(b_num,header,content,value,d_num,v_num,type)" +
                    $" values('{item.BaseNum}','{item.Header}','{item.Description}','{item.Value}','{item.DeviceNum}','{item.VariableNum}','{item.type}')";
                sqls.Add(sql);
            }

            sqls.Add("delete from sys_users");
            foreach (var item in users)
            {
                string sql = "insert into sys_users(user_name,password,user_type,real_name,gender,phone_num,department) " +
                    $"values('{item.UserName}','{item.Password}','{item.UserType}','{item.RealName}',{item.Gender},'{item.PhoneNum}','{item.Department}')";
                sqls.Add(sql);
            }

            this.SqlExecute(sqls);
        }

        public void ResetPassword(string username)
        {
            string sql = $"update sys_users set password='123456' where user_name ='{username}'";
            this.SqlExecute(new List<string> { sql });
        }
    }   
}
