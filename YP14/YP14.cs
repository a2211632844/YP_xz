using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace YP14
{
    [HotUpdate]
    public class YP14 : AbstractDynamicFormPlugIn
    {
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            List<string> FSEQ = new List<string>();
            //点击计算按钮 
            if (e.Key.EqualsIgnoreCase("F_yprl_Button"))
            {
                string FStartDate = this.Model.GetValue("F_yprl_StartDate").ToString();//起始日期
                string FEndDate = this.Model.GetValue("F_yprl_EndDate").ToString();//截止日期
                string FDepartment = "";
                string FDepartmentName = "";
                string sql = "";
                if (this.Model.GetValue("F_yprl_Department").IsNullOrEmptyOrWhiteSpace() == false)
                {
                    DynamicObject BM = this.Model.GetValue("F_yprl_Department") as DynamicObject;
                    FDepartment = BM[0].ToString();
                    FDepartmentName = BM["Name"].ToString();
                    sql = string.Format(@"exec  w_sp_grossprofitRPT_WithoutCost '{0}','{1}','{2}'", FStartDate, FEndDate, FDepartmentName);
                }
                else
                {
                    sql = string.Format(@"exec  w_sp_grossprofitRPT_WithoutCost '{0}','{1}',''", FStartDate, FEndDate);
                }


                DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sql);
                DataTable dt = ds.Tables[0];
                this.View.Model.DeleteEntryData("FEntity");
                var rEntity = this.View.Model.BusinessInfo.GetEntity("FEntity");
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        this.Model.CreateNewEntryRow(rEntity, i);
                        this.Model.SetValue("F_yprl_Date", dt.Rows[i]["日期"], i);
                        if (FDepartment.IsNullOrEmptyOrWhiteSpace() == false)//判断是否选择了部门
                        {
                            this.Model.SetValue("F_yprl_BMBM", FDepartment, i);
                        }
                        else
                        {
                            string BMsql = string.Format(@" select DEP.FDEPTID from T_BD_DEPARTMENT_L  DEPL
                join T_BD_DEPARTMENT DEP ON DEPL.FDEPTID=DEP.FDEPTID where FCREATEORGID = 1 AND FNAME= '{0}'", dt.Rows[i]["销售点"]);
                            DataSet BMds = DBServiceHelper.ExecuteDataSet(Context, BMsql);
                            DataTable BMdt = BMds.Tables[0];
                            this.Model.SetValue("F_yprl_BMBM", BMdt.Rows[0]["FDEPTID"], i);
                        }
                        this.Model.SetValue("F_yprl_CGAmount", dt.Rows[i]["新猪金额不含采购费用"], i);
                        this.Model.SetValue("F_yprl_CBAmount", dt.Rows[i]["采购入库单成本"], i);
                        if (Convert.ToInt32(dt.Rows[i]["新猪金额不含采购费用"]) == 0 && Convert.ToInt32(dt.Rows[i]["采购入库单成本"]) == 0)
                        {
                            int ss = Convert.ToInt32(dt.Rows[i]["新猪金额不含采购费用"]);
                            int qq = Convert.ToInt32(dt.Rows[i]["采购入库单成本"]);
                            FSEQ.Add(i.ToString());
                        }
                    }
                    int f = 0;
                    foreach (var item in FSEQ)
                    {
                        //this.Model.DeleteEntryRow("FEntity", Convert.ToInt32(item));
                        this.Model.DeleteEntryRow("FEntity", Convert.ToInt32(item) - f);
                        f++;
                    }
                    this.Model.ClearNoDataRow();
                    this.View.UpdateView("FEntity");
                }

            }
        }
    }
}
