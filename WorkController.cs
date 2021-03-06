﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Service
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public WorkController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Ok(DAL.WorkInfo.Instance.GetCount());
        }
        [HttpGet("new")]
        public ActionResult GetNew()
        {
            var result = DAL.Activity.Instance.GetNew();
            if (result.Count() != 0)
                return Ok(Result.Ok(result));
            else
                return Ok(Result.Err("记录数为0"));
        }
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            var result = DAL.WorkInfo.Instance.GetModel(id);
            if (result != null)
                return Ok(Result.Ok(result));
            else
                return Ok(Result.Err("WorkId不存在"));
        }
        [HttpPost]
        public ActionResult Post([FromBody] Model.WorkInfo workInfo)
        {
            workInfo.recommend = "否";
            workInfo.workVerify = "待审核";
            workInfo.uploadTime = DateTime.Now;
            try
            {
                int n = DAL.WorkInfo.Instance.Add(workInfo);
                return Ok(Result.Ok("发布作品成功", n));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("foreign key"))
                    if (ex.Message.ToLower().Contains("username"))
                        return Ok(Result.Err("合法用户才能添加记录"));
                    else
                        return Ok(Result.Err("作品所属活动不存在"));
                else if (ex.Message.ToLower().Contains("null"))
                    return Ok(Result.Err("作品名称、上传作品时间、作品图片、作品审核情况、用户名、是否推荐不能为空"));
                else
                    return Ok(Result.Err(ex.Message));
            }
        }
        [HttpPut]
        public ActionResult Put([FromBody] Model.WorkInfo workInfo)
        {
            workInfo.recommend = "否";
            workInfo.workVerify = "待审核";
            workInfo.uploadTime = DateTime.Now;
            try
            {
                var n = DAL.WorkInfo.Instance.Update(workInfo);
                if (n != 0)
                    return Ok(Result.Ok("修改作品成功", workInfo.workId));
                else
                    return Ok(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Ok(Result.Err("作品名称、上传作品时间、作品图片、作品审核情况、用户名、是否推荐不能为空"));
                else
                    return Ok(Result.Err(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                var n = DAL.WorkInfo.Instance.Delete(id);
                if (n != 0)
                    return Ok(Result.Ok("删除成功"));
                else
                    return Ok(Result.Err("workId不存在"));

            }
            catch (Exception ex)
            {
                return Ok(Result.Err(ex.Message));
            }
        }
        [HttpPost("count")]
        public ActionResult getCount([FromBody] int[] activityIds)
        {
            return Ok(DAL.WorkInfo.Instance.GetCount(activityIds));
        }
        [HttpPost("page")]
        public ActionResult getPage([FromBody] Model.WorkPage page)
        {
            var result = DAL.WorkInfo.Instance.GetPage(page);
            if (result.Count() == 0)
                return Ok(Result.Err("返回记录数为0"));
            return Ok(Result.Ok(result));
        }
        [HttpGet("findCount")]
        public ActionResult getFindCount(string findName)
        {
            if (findName == null) findName = "";
            return Ok(DAL.WorkInfo.Instance.GetFindCount(findName));
        }
        [HttpGet("myCount")]
        public ActionResult getMyCount(string userName)
        {
            if (userName == null) userName = "";
            return Ok(DAL.WorkInfo.Instance.GetMyCount(userName));
        }
        [HttpPost("findpage")]
        public ActionResult getFindPage([FromBody] Model.WorkFindPage page)
        {
            if (page.workName == null) page.workName = "";
            var result = DAL.WorkInfo.Instance.GetFindPage(page);
            if (result.Count() == 0)
                return Ok(Result.Err("返回记录数为0"));
            else
                return Ok(Result.Ok(result));
        }
        [HttpPost("mypage")]
        public ActionResult getMyPage([FromBody] Model.WorkMyPage page)
        {
            var result = DAL.WorkInfo.Instance.GetMyPage(page);
            if (result.Count() == 0)
                return Ok(Result.Err("返回记录数为0"));
            else
                return Ok(Result.Ok(result));
        }
        [HttpPut("Verify")]
        public ActionResult PutVerify([FromBody] Model.WorkInfo workInfo)
        {

            try
            {
                var n = DAL.WorkInfo.Instance.UpdateVerify(workInfo);
                if (n != 0)
                    return Ok(Result.Ok("修改作品成功", workInfo.workId));
                else
                    return Ok(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Ok(Result.Err("作品审核情况不能为空"));
                else
                    return Ok(Result.Err(ex.Message));
            }
        }
        [HttpPut("Recommend")]
        public ActionResult PutRecommend([FromBody] Model.WorkInfo workInfo)
        {

            workInfo.recommendTime = DateTime.Now;
            try
            {
                var re = "";
                if (workInfo.recommend == "否") re = "取消";
                var n = DAL.WorkInfo.Instance.UpdateRecommend(workInfo);
                if (n != 0)
                    return Ok(Result.Ok($"{re}推荐作品成功", workInfo.workId));
                else
                    return Ok(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Ok(Result.Err("推荐作品不能为空"));
                else
                    return Ok(Result.Err(ex.Message));
            }
        }
        [HttpPut("{id}")]
        public ActionResult upImg(string id, List<IFormFile> files)
        {

            var path = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "img", "work");
            var fileName = $"{path}/{id}";
            try
            {
                var ext = DAL.Upload.Instance.UpImg(files[0], fileName);
                if (ext == null)
                    return Ok(Result.Err("请上传图片文件"));
                else
                {
                    var file = $"img/Work/{id}{ext}";
                    Model.WorkInfo workInfo = new Model.WorkInfo();
                    if (id.StartsWith("i"))
                    {
                        workInfo.workId = int.Parse(id.Substring(1));
                        workInfo.workIntroduction = file;
                    }
                    else
                    {
                        workInfo.workId = int.Parse(id.Substring(1));
                        workInfo.workIntroduction = file;
                    }
                    var n = DAL.WorkInfo.Instance.UpdateImg(workInfo);
                    if (n > 0)
                        return Ok(Result.Ok("上传成功", file));
                    else
                        return Ok(Result.Err("请输入正确的作品id"));
                }
            }
            catch (Exception ex)
            {
                return Ok(Result.Err(ex.Message));
            }
        }
    }
}
    
