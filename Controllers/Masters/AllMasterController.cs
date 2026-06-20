using Dia_CoreAPI.DTOs.Masters;
using Dia_CoreAPI.Services.Interfaces.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dia_CoreAPI.Controllers.Masters
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AllMasterController : ControllerBase
    {
        private readonly IAllMasterService _masterService;
        private readonly I4CMasterService _4cmasterService;

        public AllMasterController(IAllMasterService masterService, I4CMasterService fourCMasterService)
        {
            _masterService = masterService;
            _4cmasterService = fourCMasterService;
        }

        private string GetUserId() =>  HttpContext.User.FindFirst("UserID")?.Value ?? "0";
        private string GetUserTypeId() => HttpContext.User.FindFirst("UserTypeId")?.Value ?? "0";
        private string GetCompanyId() => HttpContext.User.FindFirst("CompanyID")?.Value ?? "0";

        #region All Master
        [HttpGet("all-master-list")]
        public async Task<IActionResult> GetAllMasterList()
        {
            try
            {
                var result =
                    await _masterService.GetAllMasterListAsync(GetUserTypeId(), GetCompanyId());
                
                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }
        #endregion

        #region Shape Master

        [HttpGet("list-shape")]
        public async Task<IActionResult> ShapeMaster()
        {
            try
            {
                var result =
                    await _4cmasterService.GetShapeMasterListAsync("",GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpGet("get-shape/{id}")]
        public async Task<IActionResult> GetShape(int id)
        {
            try
            {
                var data = await _4cmasterService.GetShapeMasterListAsync($" AND ID = {id}",GetCompanyId());
                return Ok(new
                {
                    Success = true,
                    Data = data.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("save-shape")]
        public async Task<IActionResult> SaveShape([FromBody] ShapeMaster model)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission =
                        await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(),"Shape Master", "insert", GetCompanyId());

                    if (permission == 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "You are not authorized to perform this task."
                        });
                    }

                    var duplicateData =
                        await _4cmasterService.GetShapeMasterListAsync($" AND ShapeID = '{model.ShapeID}' ", GetCompanyId());

                    if (duplicateData.Count > 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "Record already available with shape id - " + model.ShapeID
                        });
                    }

                    bool result =
                        await _4cmasterService.SaveShapeMasterAsync(model,GetUserId(),GetCompanyId(),"insert");

                    return Ok(new
                    {
                        Success = result,
                        Data = "Record saved successfully."
                    });
                }
                else
                {
                    int permission =
                        await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(), "Shape Master", "edit", GetCompanyId());

                    if (permission == 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "You are not authorized to perform this task."
                        });
                    }

                    var duplicateData =
                        await _4cmasterService.GetShapeMasterListAsync($" AND ShapeID = '{model.ShapeID}' AND ID <> {model.ID}", GetCompanyId());

                    if (duplicateData.Count > 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "Sorry.! You can not update record because shape id already exists."
                        });
                    }

                    bool result =
                        await _4cmasterService.SaveShapeMasterAsync(model,GetUserId(),GetCompanyId(),"update");

                    return Ok(new
                    {
                        Success = result,
                        Data = "Record updated successfully."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("delete-shape/{id}")]
        public async Task<IActionResult> DeleteShape(int id)
        {
            try
            {
                bool result =
                    await _4cmasterService.DeleteShapeAsync(id, GetUserTypeId(), GetUserId(), GetCompanyId());

                return Ok(new
                {
                    Success = result,
                    Data = result
                        ? "Records Deleted."
                        : "Record is in use, You can not delete record."
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        //[HttpPost("delete-shape/{id}")]
        //public async Task<IActionResult> DeleteShape(int id)
        //{
        //    try
        //    {
        //        int permission =
        //            await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(),"Shape Master","delete",GetCompanyId());

        //        if (permission != 1)
        //        {
        //            return Ok(new
        //            {
        //                Success = false,
        //                Data = "You are not authorized to perform this task."
        //            });
        //        }

        //        var data =
        //            (await _4cmasterService.GetShapeMasterListAsync(" AND ID = " + id, GetCompanyId())).SingleOrDefault();

        //        if (data == null)
        //        {
        //            return Ok(new
        //            {
        //                Success = false,
        //                Data = "Record not found."
        //            });
        //        }

        //        bool result =
        //            await _4cmasterService.SaveShapeMasterAsync(data, GetUserId(), GetCompanyId(),"delete");

        //        if (result)
        //        {
        //            return Ok(new
        //            {
        //                Success = true,
        //                Data = "Records Deleted."
        //            });
        //        }

        //        return Ok(new
        //        {
        //            Success = false,
        //            Data = "Record is in use, You can not delete record."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new
        //        {
        //            Success = false,
        //            Data = ex.Message
        //        });
        //    }
        //}



        //[HttpPost("delete-shape/{id}")]
        //public async Task<IActionResult> DeleteShape(int id)
        //{
        //    try
        //    {
        //        int permission =
        //    await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(),"Shape Master","delete", GetCompanyId());

        //        if (permission == 0)
        //        {
        //            return Ok(new
        //            {
        //                Success = false,
        //                Data = "You are not authorized to perform this task."
        //            });
        //        }

        //        var data = (await _4cmasterService.GetShapeMasterListAsync($" AND ID = {id}",GetCompanyId())).FirstOrDefault();

        //        if (data == null)
        //        {
        //            return Ok(new
        //            {
        //                Success = false,
        //                Data = "Record not found."
        //            });
        //        }

        //        bool result =
        //            await _4cmasterService.SaveShapeMasterAsync(data,GetUserId(),GetCompanyId(),"delete");

        //        return Ok(new
        //        {
        //            Success = result,
        //            Data = result
        //                ? "Records Deleted."
        //                : "Record is in use."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Ok(new
        //        {
        //            Success = false,
        //            Data = ex.Message
        //        });
        //    }
        //}

        #endregion

        #region Color Master

        [HttpGet("list-color")]
        public async Task<IActionResult> ColorMaster()
        {
            try
            {
                var result =
                    await _4cmasterService.GetColorMasterListAsync("", GetCompanyId());

                return Ok(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpGet("get-color/{id}")]
        public async Task<IActionResult> GetColor(int id)
        {
            try
            {
                var data = await _4cmasterService.GetColorMasterListAsync($" AND ID = {id}", GetCompanyId());
                return Ok(new
                {
                    Success = true,
                    Data = data.FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpPost("save-color")]
        public async Task<IActionResult> SaveColor([FromBody] ColorMaster model)
        {
            try
            {
                if (model.ID == 0)
                {
                    int permission =
                        await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(), "Color Master", "insert", GetCompanyId());

                    if (permission == 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "You are not authorized to perform this task."
                        });
                    }

                    var duplicateData =
                        await _4cmasterService.GetColorMasterListAsync($" AND ColorId = '{model.ColorID}' ", GetCompanyId());

                    if (duplicateData.Count > 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "Record already available with color id - " + model.ColorID
                        });
                    }

                    bool result =
                        await _4cmasterService.SaveColorMasterAsync(model, GetUserId(), GetCompanyId(), "insert");

                    return Ok(new
                    {
                        Success = result,
                        Data = "Record saved successfully."
                    });
                }
                else
                {
                    int permission =
                        await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(), "Color Master", "edit", GetCompanyId());

                    if (permission == 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "You are not authorized to perform this task."
                        });
                    }

                    var duplicateData =
                        await _4cmasterService.GetColorMasterListAsync($" AND ColorId = '{model.ColorID}' AND ID <> {model.ID}", GetCompanyId());

                    if (duplicateData.Count > 0)
                    {
                        return Ok(new
                        {
                            Success = false,
                            Data = "Sorry.! You can not update record because shape id already exists."
                        });
                    }

                    bool result =
                        await _4cmasterService.SaveColorMasterAsync(model, GetUserId(), GetCompanyId(), "update");

                    return Ok(new
                    {
                        Success = result,
                        Data = "Record updated successfully."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }

        [HttpDelete("delete-color/{id}")]
        public async Task<IActionResult> DeleteColor(int id)
        {
            try
            {
                int permission =
                    await _4cmasterService.CheckUserPermissionAsync(GetUserTypeId(), "Color Master", "delete", GetCompanyId());

                if (permission != 1)
                {
                    return Ok(new
                    {
                        Success = false,
                        Data = "You are not authorized to perform this task."
                    });
                }

                var data =
                    (await _4cmasterService.GetColorMasterListAsync(" AND ID = " + id, GetCompanyId())).SingleOrDefault();

                if (data == null)
                {
                    return Ok(new
                    {
                        Success = false,
                        Data = "Record not found."
                    });
                }

                bool result =
                    await _4cmasterService.SaveColorMasterAsync(data, GetUserId(), GetCompanyId(), "delete");

                if (result == true)
                {
                    return Ok(new
                    {
                        Success = true,
                        Data = "Records Deleted."
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Success = false,
                        Data = "Record is in use, You can not delete record."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Success = false,
                    Data = ex.Message
                });
            }
        }



        #endregion
    }
}