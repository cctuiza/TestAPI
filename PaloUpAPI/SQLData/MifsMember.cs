using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PaloUpAPI.Models;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace PaloUpAPI.SQLData
{
    public class MifsMember
    {

      


        public dynamic get(int pg, int tk,string fnd)
        {
           
            try
            {
                TransList transList = new TransList();
                int skip = tk * (pg - 1); // skip the record

                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    if (string.IsNullOrEmpty(fnd))
                    {
                        var meetingMembers = db.mifs_members
                         .OrderBy(od => od.fullname)
                   .Skip(skip)
                   .Take(tk)
                   .ToList();

                        transList.total_count = db.mifs_members.Count();
                        transList.data = meetingMembers;
                    }
                    else
                    {
                        var meetingMembers = db.mifs_members
                        .Where(wr => wr.fullname.Contains(fnd) || wr.emailaddress.Contains(fnd) )
                         .OrderBy(od => od.fullname)
                           .Skip(skip)
                           .Take(tk)
                           .ToList();

                        transList.total_count = db.mifs_members.Where(wr => wr.fullname.Contains(fnd) || wr.emailaddress.Contains(fnd) ).Count();
                        transList.data = meetingMembers;
                    }
                   
                    return transList;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public dynamic get(string p)
        {
            try
            {
                using (PALOUPEntities db = new PALOUPEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;

                    var meetingMember = db.mifs_members.Where(wr => wr.memberId == p).FirstOrDefault();


                    return meetingMember;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      

   
       

        /// <summary>
        /// Get all employee
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="size">sire per row</param>
        /// <returns>IEnumerable</returns>
        public EmployeeModel GetAllEmployee(int page, int size)
        {
            EmployeeModel empModel = new EmployeeModel();
            try
            {
                EmployeeAccount empPage = new EmployeeAccount();
                List<EmployeeAccount> empAccount = new List<EmployeeAccount>();


                PALOUPEntities db = new PALOUPEntities();

                var qMemberList = db.mifs_members.ToList().Select(aa => new { aa.memberId });
                List<string> emplIdList = new List<string>();

                foreach (var row in qMemberList)
                    emplIdList.Add(row.memberId);



                int ipage = 1;

                if (page == 0)
                {
                    ipage = 1;//set default page to 1 if the page is equal to 0
                }
                else
                {
                    ipage = page;
                }

                int total = db.core_employee.Where(p => p.type != "SEP")
                    .Where(wr => !emplIdList.Contains(wr.empl_id))
                    .Select(p => p.empl_id).Count();

                int pageSize = size; // set your page size, which is number of records per page

                int skip = pageSize * (ipage - 1); // skip the record

                bool canPage = skip < total;// bool

                int pageCount = total / size;


                var q_emplist = (from aa in db.core_employee
                                 where aa.type != "SEP" && !emplIdList.Contains(aa.empl_id)
                                 select new { aa.empl_id, aa.corporate, aa.employeename2, aa.emailadd, aa.positionname, aa.employeepresentcontact }).OrderBy(p => p.employeename2).Skip(skip).Take(pageSize);

                foreach (var row in q_emplist)
                {
                    empAccount.Add(new EmployeeAccount
                    {
                        EmpID = row.empl_id,
                        SBU = row.corporate,
                        EmpName = row.employeename2,
                        EmpEmail = row.emailadd,
                        Position = row.positionname,
                        UrlID = "http://apps.fastgroup.biz/201pic/96px/" + row.empl_id + ".jpg",
                        ContactNo = row.employeepresentcontact == string.Empty ? "" : row.employeepresentcontact
                    });
                }


                empModel.empAccount = empAccount;
                empModel.ipage = page;
                empModel.canPage = canPage;
                empModel.pageCount = pageCount;
                empModel.hasError = false;




                return empModel;
            }
            catch (Exception ex)
            {
                empModel.hasError = true;
                empModel.error_message = ex.Message;
                return empModel;
            }
        }

        /// <summary>
        /// Search Employee
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="size">sire per row</param>
        /// <param name="searchParam">search parameter</param>
        /// <returns>return list of search result</returns>
        public EmployeeModel GetSearchEmployee(int page, int size, string searchParam)
        {
            EmployeeModel empModel = new EmployeeModel();
            try
            {
                EmployeeAccount empPage = new EmployeeAccount();
                List<EmployeeAccount> empAccount = new List<EmployeeAccount>();


                PALOUPEntities db = new PALOUPEntities();

                var qMemberList = db.mifs_members.ToList().Select(aa => new { aa.memberId });
                List<string> emplIdList = new List<string>();

                foreach (var row in qMemberList)
                    emplIdList.Add(row.memberId);


                int ipage = 1;

                if (page == 0)
                {
                    ipage = 1;//set default page to 1 if the page is equal to 0
                }
                else
                {
                    ipage = page;
                }

                int total = db.core_employee
                    .Where(p => p.type != "SEP" && (p.employeename2.Contains(searchParam) || p.empl_id.Contains(searchParam) || p.emailadd.Contains(searchParam)))
                    .Where(wr => !emplIdList.Contains(wr.empl_id))
                    .Select(p => p.empl_id).Count();

                int pageSize = size; // set your page size, which is number of records per page

                int skip = pageSize * (ipage - 1); // skip the record

                bool canPage = skip < total;// bool

                int pageCount = total / size;

                var q_emplist = (from aa in db.core_employee
                                 where aa.type != "SEP" &&   !emplIdList.Contains(aa.empl_id) && (aa.employeename2.Contains(searchParam) || aa.empl_id.Contains(searchParam) || aa.emailadd.Contains(searchParam) )
                                 select new { aa.empl_id, aa.corporate, aa.employeename2, aa.emailadd, aa.positionname, aa.employeepresentcontact }).OrderBy(p => p.employeename2).Skip(skip).Take(pageSize);

                foreach (var row in q_emplist)
                {
                    empAccount.Add(new EmployeeAccount
                    {
                        EmpID = row.empl_id,
                        SBU = row.corporate,
                        EmpName = row.employeename2,
                        EmpEmail = row.emailadd,
                        Position = row.positionname,
                        UrlID = "http://apps.fastgroup.biz/201pic/96px/" + row.empl_id + ".jpg",
                        ContactNo = row.employeepresentcontact == string.Empty ? "" : row.employeepresentcontact
                    });
                }


                empModel.empAccount = empAccount;
                empModel.ipage = page;
                empModel.canPage = canPage;
                empModel.pageCount = pageCount;
                empModel.hasError = false;




                return empModel;
            }
            catch (Exception ex)
            {
                empModel.hasError = true;
                empModel.error_message = ex.Message;
                return empModel;
            }
        }

        public EmployeeModel GetAllEmployeeHris(int page, int size)
        {
            EmployeeModel empModel = new EmployeeModel();
            try
            {
                EmployeeAccount empPage = new EmployeeAccount();
                List<EmployeeAccount> empAccount = new List<EmployeeAccount>();

                PALOUPEntities db = new PALOUPEntities();
  
                int ipage = 1;

                if (page == 0)
                {
                    ipage = 1;//set default page to 1 if the page is equal to 0
                }
                else
                {
                    ipage = page;
                }

                int total = db.core_employee.Where(p => p.type != "SEP")
                    .Select(p => p.empl_id).Count();

                int pageSize = size; // set your page size, which is number of records per page

                int skip = pageSize * (ipage - 1); // skip the record

                bool canPage = skip < total;// bool

                int pageCount = total / size;


                var q_emplist = (from aa in db.core_employee
                                 where aa.type != "SEP"
                                 select new { aa.empl_id, aa.corporate, aa.employeename2, aa.emailadd, aa.positionname, aa.employeepresentcontact }).OrderBy(p => p.employeename2).Skip(skip).Take(pageSize);

                foreach (var row in q_emplist)
                {
                    empAccount.Add(new EmployeeAccount
                    {
                        EmpID = row.empl_id,
                        SBU = row.corporate,
                        EmpName = row.employeename2,
                        EmpEmail = row.emailadd,
                        Position = row.positionname,
                        UrlID = "http://apps.fastgroup.biz/201pic/96px/" + row.empl_id + ".jpg",
                        ContactNo = row.employeepresentcontact == string.Empty ? "" : row.employeepresentcontact
                    });
                }


                empModel.empAccount = empAccount;
                empModel.ipage = page;
                empModel.canPage = canPage;
                empModel.pageCount = pageCount;
                empModel.hasError = false;




                return empModel;
            }
            catch (Exception ex)
            {
                empModel.hasError = true;
                empModel.error_message = ex.Message;
                return empModel;
            }
        }

        public EmployeeModel GetSearchEmployeeHris(int page, int size, string searchParam)
        {
            EmployeeModel empModel = new EmployeeModel();
            try
            {
                EmployeeAccount empPage = new EmployeeAccount();
                List<EmployeeAccount> empAccount = new List<EmployeeAccount>();


                PALOUPEntities db = new PALOUPEntities();

              

                int ipage = 1;

                if (page == 0)
                {
                    ipage = 1;//set default page to 1 if the page is equal to 0
                }
                else
                {
                    ipage = page;
                }

                int total = db.core_employee
                    .Where(p => p.type != "SEP" && (p.employeename2.Contains(searchParam) || p.empl_id.Contains(searchParam) || p.emailadd.Contains(searchParam)))
                    .Select(p => p.empl_id).Count();

                int pageSize = size; // set your page size, which is number of records per page

                int skip = pageSize * (ipage - 1); // skip the record

                bool canPage = skip < total;// bool

                int pageCount = total / size;

                var q_emplist = (from aa in db.core_employee
                                 where aa.type != "SEP"  && (aa.employeename2.Contains(searchParam) || aa.empl_id.Contains(searchParam) || aa.emailadd.Contains(searchParam))
                                 select new { aa.empl_id, aa.corporate, aa.employeename2, aa.emailadd, aa.positionname, aa.employeepresentcontact }).OrderBy(p => p.employeename2).Skip(skip).Take(pageSize);

                foreach (var row in q_emplist)
                {
                    empAccount.Add(new EmployeeAccount
                    {
                        EmpID = row.empl_id,
                        SBU = row.corporate,
                        EmpName = row.employeename2,
                        EmpEmail = row.emailadd,
                        Position = row.positionname,
                        UrlID = "http://apps.fastgroup.biz/201pic/96px/" + row.empl_id + ".jpg",
                        ContactNo = row.employeepresentcontact == string.Empty ? "" : row.employeepresentcontact
                    });
                }


                empModel.empAccount = empAccount;
                empModel.ipage = page;
                empModel.canPage = canPage;
                empModel.pageCount = pageCount;
                empModel.hasError = false;

                return empModel;
            }
            catch (Exception ex)
            {
                empModel.hasError = true;
                empModel.error_message = ex.Message;
                return empModel;
            }
        }
    }
}