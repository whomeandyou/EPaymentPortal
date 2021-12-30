using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using Newtonsoft.Json;
using TMLM.EPayment.BL.Service;
using TMLM.EPayment.BL.Data;

namespace TMLM.EPayment.WebApi
{
    public class TMLM_LogHandler : DelegatingHandler
    {
        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {

            string _strRequestMethod = null;
            string _strRequestContent = null;
            try
            {
                System.Web.Http.Routing.IHttpRouteData _routeData = request.GetRouteData();
                if (_routeData != null && _routeData.Values.Count > 0)
                    _strRequestMethod = _routeData.Values["action"].ToString();
                else
                    _strRequestMethod = null;
                _strRequestContent = request.Content.ReadAsStringAsync().Result;
            }
            catch(Exception)
            {
                _strRequestMethod = null;
            }
            //string _strRequestMethod = request.GetRouteData().Values["action"].ToString();
            //string _strRequestContent = request.Content.ReadAsStringAsync().Result;

            if (_strRequestMethod != null)
            {
                dynamic _dynTemp = JsonConvert.DeserializeObject<System.Dynamic.ExpandoObject>(_strRequestContent);
                if (_dynTemp != null)
                {
                    /*-- replace base64 image datapacket to avoid overflow --*/
                    if (((IDictionary<string, object>)_dynTemp).ContainsKey("base64_Image"))
                    {
                        _dynTemp.Avatar_Img = "<image/>";
                        _strRequestContent = JsonConvert.SerializeObject(_dynTemp);
                    }
                }
            }

            var response = await base.SendAsync(request, cancellationToken);
            //return response;

            if (_strRequestMethod != null)
            {
                string _strResponseContent = null;
                if (!response.IsSuccessStatusCode)
                    _strResponseContent = string.Format("Status Code: {0}, Reason: {1}", (int)response.StatusCode, response.ReasonPhrase);
                else
                    _strResponseContent = response.Content.ReadAsStringAsync().Result;

                try
                {
                    using (LogApiService _svcLogApi = new LogApiService())
                    {
                        string _strLogin_Id = null;

                        AuthorizeModel _model = JsonConvert.DeserializeObject<AuthorizeModel>(_strRequestContent);
                        if (_model != null)
                        {
                            if (!string.IsNullOrEmpty(_model.Auth_Token))
                            {
                                using (MemberService _svcMember = new MemberService())
                                {
                                    GetProfileOutputModel _eMemberInfo = _svcMember.getProfile_By_Auth_Token(_model);
                                    if (_eMemberInfo != null) _strLogin_Id = _eMemberInfo.Login_Id;
                                }
                            }
                        }

                        _svcLogApi.WriteLog(_strLogin_Id, _strRequestMethod, _strRequestContent, _strResponseContent);
                    }
                }
                catch (Exception) { return response; }
            }
            return response;
        }
    }
}