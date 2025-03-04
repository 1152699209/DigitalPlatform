using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class AlarmEntity
    {

        public string CNum { get; set; }// 报警条件编辑


        [Column("a_num")]
        public string id { get; set; }
        [Column(name: "d_num")]
        public string DeviceNum { get; set; }
        [Column(name: "d_header")]
        public string DeviceName { get; set; }
        [Column(name: "v_num")]
        public string VariableNum { get; set; }
        [Column(name: "v_header")]
        public string VariableName { get; set; }
        [Column(name: "alarm_value")]
        public string RecordValue { get; set; }
        /// <summary>
        /// 记录类型：正常0、报警10、报警已处理1、联控2
        /// </summary>
        [Column(name: "state")]
        public string State { get; set; }
        [Column(name: "a_num")]
        public string AlarmNum { get; set; }
        [Column(name: "date_time")]
        public string RecordTime { get; set; }
        [Column("solve_time")]
        public string SolveTime { get; set; }
        [Column(name: "content")]
        public string AlarmContent { get; set; }
        [Column(name: "level")]
        public string AlarmLevel { get; set; }
        /// <summary>
        /// 操作员
        /// </summary>
        [Column(name: "user_id")]
        public string UserId { get; set; }
        [Column(name: "real_name")]
        public string UserName { get; set; }
    }
}
