using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMLM.BL.Data;
using TMLM.EPayment.BL.Helpers;

namespace TMLM.EPayment.BL.PaymentProvider.MPGS
{
    public class GatewayApiRequest
    {
        private GatewayApiConfig gatewayApiConfig;

        public String OrderNo { get; set; }
        public String TransactionId { get; set; }

        public String ApiOperation { get; set; }
        public String ApiMethod { get; set; }
        public String RequestUrl { get; set; }
        public String Payload { get; set; }

        public String SessionId { get; set; }
        public String SecureId { get; set; }
        public String SecureIdResponseUrl { get; set; }

        public String SourceType { get; set; }
        public String CardNumber { get; set; }
        public String ExpiryMonth { get; set; }
        public String ExpiryYear { get; set; }
        public String SecurityCode { get; set; }

        public String Amt { get; set; }
        public String OrderCurrency { get; set; }
        public String OrderDescription { get; set; }

        public String OrderReference { get; set; }
        public String TransactionReference { get; set; }

        public String TransactionAmount { get; set; }
        public String TransactionCurrency { get; set; }
        public String TargetTransactionId { get; set; }

        public String ReturnUrl { get; set; }
        public String Operation { get; set; }

        public String BankName { get; set; }

        public String PaymentAuthResponse { get; set; }
        public String SecureId3D { get; set; }


        public String BrowserPaymentOperation { get; set; }
        public String BrowserPaymentPaymentConfirmation { get; set; }

        public String PaymentRef { get; set; }

        public string AId { get; set; }
        public string MsgSign { get; set; }

        public bool IsDifferentRenewalMethod { get; set; }
        public List<DropDownListModel> MonthDropdownlist { get; set; }
        public List<DropDownListModel> YearDropdownlist { get; set; }
        public String MasterpassOnline { get; set; }
        public String MasterpassOriginUrl { get; set; }

        public String MasterpassOauthToken { get; set; }
        public String MasterpassOauthVerifier { get; set; }
        public String MasterpassCheckoutUrl { get; set; }

        public bool IsInitialPayment { get; set; }

        public bool IsEnrolment { get; set; }

        public String PayorName { get; set; }

        public String statementDescriptorName { get; set; }

        public String Token { get; set; }

        private String apiBaseUrl { get; set; }

        public string CustomerLastName { get; set; }
        public string ApplicationAccountCode { get; set; }
        public bool InitiateAuthentication { get; set; }
        public bool PayerAuthenticate { get; set; }

        public string innerWidth { get; set; }
        public string innerHeight { get; set; }
        public string colorDepth { get; set; }
        public string javaEnabled { get; set; }
        public string browserName { get; set; }
        public string timeZone { get; set; }
        public string language { get; set; }

        public string dsTransactionId { get; set; }
        public string dsVersion { get; set; }
        public string authenticationToken { get; set; }
        public string veres { get; set; }
        public string pares { get; set; }
        public string acseci { get; set; }
        public string transactionStatus { get; set; }
        public string statusReasonCode { get; set; }
        public string newTransactionId { get; set; }
        public string agreementId { get; set; }
        public string storeOnFile { get; set; }

        public GatewayApiRequest()
        {
        }

        public GatewayApiRequest(GatewayApiConfig gatewayApiConfig)
        {
            GatewayApiConfig = gatewayApiConfig;
        }

        public GatewayApiConfig GatewayApiConfig
        {
            //get => gatewayApiConfig;
            //set => gatewayApiConfig = value;
            get { return gatewayApiConfig; }

            set { gatewayApiConfig = value; }
        }

        public string buildRequestUrl()
        {
            RequestUrl = $@"{GatewayApiConfig.GatewayUrl}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/order/{OrderNo}/transaction/{TransactionId}";
            return RequestUrl;
        }

        public string buildSecureIdRequestUrl()
        {
            string url = $@"{GatewayApiConfig.GatewayUrl}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/3DSecureId";
            if (!String.IsNullOrEmpty(SecureId))
            {
                url = $"{url}/{SecureId}";
            }
            RequestUrl = url;
            return RequestUrl;
        }

        public string buildSessionRequestUrl()
        {
            string url = $@"{GatewayApiConfig.GatewayUrl}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/session";
            if (!String.IsNullOrEmpty(SessionId))
            {
                url = $"{url}/{SessionId}";
            }
            RequestUrl = url;
            return RequestUrl;
        }

        public string buildRetrieveTransactionRequestUrl(string orderId,string transactionId)
        {
            string url = $@"{GatewayApiConfig.GatewayUrl}/api/rest/version/{GatewayApiConfig.Version}/merchant/{GatewayApiConfig.MerchantId}/order/{orderId}/transaction/{transactionId}";
          
            RequestUrl = url;
            return RequestUrl;
        }

        public static GatewayApiRequest createApiRequest(GatewayApiConfig gatewayApiConfig, string apiOperation = null, string sessionId = null)
        {
            
            GatewayApiRequest gatewayApiRequest = new GatewayApiRequest(new GatewayApiConfig())
            {
                SessionId = sessionId,
                //OrderId = IdUtils.generateSampleId(),
                //TransactionId = IdUtils.generateSampleId(),
                ////ApiOperation = apiOperation,
                //OrderAmount = "1",
                //OrderCurrency = gatewayApiConfig.Currency,
                //OrderDescription = "TESTING ORDER DESCRIPTION!",
                //OrderReference = "TESTING ORDER REF!",
                //TransactionReference = IdUtils.generateSampleId(),
                ////SecureId = IdUtils.generateSampleId(),
                ////SecureId3D = IdUtils.generateSampleId(),

                ////ApiMethod = "PUT",
                //ApiOperation = "PAY",
                //SourceType = "CARD",
                //CardNumber = "5123450000000008",
                //ExpiryMonth = "05",
                //ExpiryYear = "21",
                //SecurityCode = "100"

                CardNumber = "4508750015741019",
                ExpiryMonth = "05",
                ExpiryYear = "21",
                SecurityCode = "100",
                OrderNo = IdUtils.generateSampleId(),
                Amt = "1",
                OrderCurrency = "MYR",
                OrderDescription = "TESTING ORDER DESCRIPTION!",
                TransactionId = IdUtils.generateSampleId(),
                SecureId3D = "201909199781",
                OrderReference = "TESTING ORDER REF!",
                TransactionReference = IdUtils.generateSampleId()

            };

            return gatewayApiRequest;
        }

        /// <summary>
        /// return the correct API gateway base URL, depends on the authentication method.
        /// </summary>
        /// <returns>The API gateway base URL.</returns>
        public string getApiGatewayBaseURL()
        {
            if (apiBaseUrl == null)
            {
                if (GatewayApiConfig.AuthenticationByCertificate)
                {
                    apiBaseUrl = GatewayApiConfig.GatewayUrlCertificate;
                }
                else
                {
                    apiBaseUrl = GatewayApiConfig.GatewayUrl;
                }
            }
            return apiBaseUrl;
        }


        /// <summary>
        /// Builds the JSON payload.
        /// </summary>
        /// <returns>The JSON payload.</returns>
        public string buildPayload()
        {
            NameValueCollection nvc = new NameValueCollection();

            if(InitiateAuthentication)
            {
                if(MPGSSettings.Enable3DS2)
                {
                    nvc.Add("authentication.acceptVersions", "3DS1,3DS2");
                }
                else
                {
                    nvc.Add("authentication.acceptVersions", "3DS1");
                }
             
                nvc.Add("authentication.channel", "PAYER_BROWSER");
                nvc.Add("authentication.purpose", "PAYMENT_TRANSACTION");
                nvc.Add("transaction.reference", OrderNo);

                if (!String.IsNullOrEmpty(ApiOperation))
                {
                    nvc.Add("apiOperation", ApiOperation);
                }

                if (!String.IsNullOrEmpty(CardNumber))
                {
                    nvc.Add("sourceOfFunds.provided.card.number", CardNumber);
                }

                if (!String.IsNullOrEmpty(OrderCurrency))
                {
                    nvc.Add("order.currency", OrderCurrency);
                }

                Payload = JsonHelper.BuildJsonFromNVC(nvc);

                return Payload;
            }

            if(PayerAuthenticate)
            {
                nvc.Add("authentication.redirectResponseUrl", SecureIdResponseUrl);
                //nvc.Add("correlationId", "test");

                nvc.Add("device.browser", browserName);
                nvc.Add("device.browserDetails.3DSecureChallengeWindowSize", "FULL_SCREEN");
                nvc.Add("device.browserDetails.acceptHeaders", "application/json");
                nvc.Add("device.browserDetails.colorDepth", colorDepth);
                nvc.Add("device.browserDetails.javaEnabled", javaEnabled);
                nvc.Add("device.browserDetails.language", language);
                nvc.Add("device.browserDetails.screenHeight", innerHeight);
                nvc.Add("device.browserDetails.screenWidth", innerWidth);
                nvc.Add("device.browserDetails.timeZone", timeZone);

                //nvc.Add("device.ipAddress", "192.168.68.106");

                //nvc.Add("order.id", OrderNo);
                nvc.Add("order.amount", Amt);
                nvc.Add("order.currency", OrderCurrency);

                if (!String.IsNullOrEmpty(SessionId))
                {
                    nvc.Add("session.id", SessionId);
                }

                //nvc.Add("sourceOfFunds.provided.card.number", CardNumber);
                //nvc.Add("sourceOfFunds.provided.card.expiry.month", ExpiryMonth);
                //nvc.Add("sourceOfFunds.provided.card.expiry.year", ExpiryYear);

                nvc.Add("apiOperation", ApiOperation);
                Payload = JsonHelper.BuildJsonFromNVC(nvc);

                return Payload;
            }

            if (!String.IsNullOrEmpty(ApiOperation))
            {
                nvc.Add("apiOperation", ApiOperation);
            }

            if ("PAY" == ApiOperation && dsVersion == "3DS2")
            {
                nvc.Add("authentication.3ds.transactionId", dsTransactionId);
                nvc.Add("authentication.3ds.authenticationToken", authenticationToken);
                nvc.Add("authentication.3ds2.transactionStatus", transactionStatus);
                nvc.Add("authentication.3ds.acsEci", acseci);
                if(!string.IsNullOrEmpty(statusReasonCode))
                {
                    nvc.Add("authentication.3ds2.statusReasonCode", statusReasonCode);
                }
                //nvc.Add("agreement.id", agreementId);
               // nvc.Add("sourceOfFunds.provided.card.storedOnFile", "TO_BE_STORED");
            }

            if ("PAY" == ApiOperation && dsVersion == "3DS1")
            {
                nvc.Add("authentication.3ds.transactionId", dsTransactionId);
                nvc.Add("authentication.3ds.acsEci", acseci);
                nvc.Add("authentication.3ds.authenticationToken", authenticationToken);
                nvc.Add("authentication.3ds1.veResEnrolled", veres);
                if (!string.IsNullOrEmpty(pares))
                {
                    nvc.Add("authentication.3ds1.paResStatus", pares);
                }
                //nvc.Add("agreement.id", agreementId);
                //nvc.Add("sourceOfFunds.provided.card.storedOnFile", "TO_BE_STORED");
            }
            if("PAY" == ApiOperation && string.IsNullOrEmpty(dsVersion))
            {
                nvc.Add("sourceOfFunds.provided.card.expiry.month", ExpiryMonth);
                nvc.Add("sourceOfFunds.provided.card.expiry.year", ExpiryYear);
            }

            //if (("PAY" == ApiOperation || "VERIFY" == ApiOperation) && dsVersion == "3DS1")
            //{
            //    nvc.Add("authentication.3ds.transactionId", dsTransactionId);
            //    nvc.Add("authentication.3ds.acsEci", acseci);
            //    nvc.Add("authentication.3ds.authenticationToken", authenticationToken);
            //    nvc.Add("authentication.3ds1.veResEnrolled", veres);
            //    nvc.Add("agreement.id", agreementId);
            //    nvc.Add("sourceOfFunds.provided.card.storedOnFile", "TO_BE_STORED");
            //}

            //if ("VERIFY" == ApiOperation)
            //{
            //    nvc.Add("agreement.id", agreementId);
            //    nvc.Add("sourceOfFunds.provided.card.storedOnFile", "TO_BE_STORED");
            //}

            if (!String.IsNullOrEmpty(SecureId3D))
            {
                nvc.Add("3DSecureId", SecureId3D);
            }

            if (!String.IsNullOrEmpty(SessionId))
            {
                nvc.Add("session.id", SessionId);
            }

            if (!String.IsNullOrEmpty(Token))
            {
                nvc.Add("sourceOfFunds.token", Token);
            }

            if (!String.IsNullOrEmpty(CardNumber))
            {
                nvc.Add("sourceOfFunds.provided.card.number", CardNumber);
                nvc.Add("sourceOfFunds.provided.card.expiry.month", ExpiryMonth);
                nvc.Add("sourceOfFunds.provided.card.expiry.year", ExpiryYear);
                // remove 3ds enrollment
                //if ("CHECK_3DS_ENROLLMENT" != ApiOperation)
                //{
                //    nvc.Add("sourceOfFunds.provided.card.securityCode", SecurityCode);
                //}
            }

            if (!String.IsNullOrEmpty(SourceType))
            {
                nvc.Add("sourceOfFunds.type", SourceType);
            }

            if ("CREATE_CHECKOUT_SESSION" == ApiOperation || String.IsNullOrEmpty(ApiOperation) || "UPDATE_SESSION_FROM_WALLET" == ApiOperation)
            {
                // Need to add order ID in the request body for CREATE_CHECKOUT_SESSION or UPDATE SESSION. 
                // Its presence in the body will cause an error for the other operations.
                if (!String.IsNullOrEmpty(OrderNo))
                {
                    nvc.Add("order.id", OrderNo);
                }

                //masterpass
                if (String.IsNullOrEmpty(ApiOperation) || "CREATE_CHECKOUT_SESSION" != ApiOperation)
                {

                    if (!String.IsNullOrEmpty(MasterpassOnline))
                    {
                        nvc.Add("order.walletProvider", MasterpassOnline);
                    }
                    if (!String.IsNullOrEmpty(MasterpassOriginUrl))
                    {
                        nvc.Add("wallet.masterpass.originUrl", MasterpassOriginUrl);
                    }
                    if (!String.IsNullOrEmpty(MasterpassOauthToken))
                    {
                        nvc.Add("wallet.masterpass.oauthToken", MasterpassOauthToken);
                    }
                    if (!String.IsNullOrEmpty(MasterpassOauthVerifier))
                    {
                        nvc.Add("wallet.masterpass.oauthVerifier", MasterpassOauthVerifier);
                    }
                    if (!String.IsNullOrEmpty(MasterpassCheckoutUrl))
                    {
                        nvc.Add("wallet.masterpass.checkoutUrl", MasterpassCheckoutUrl);
                    }
                }

                if (!String.IsNullOrEmpty(ReturnUrl) && "CREATE_CHECKOUT_SESSION" == ApiOperation)
                {
                    nvc.Add("interaction.returnUrl", ReturnUrl);
                    nvc.Add("interaction.operation",  Operation);
                }

            }

            if (!String.IsNullOrEmpty(OrderReference))
            {
                nvc.Add("order.reference", OrderReference);
            }

            if (!String.IsNullOrEmpty(Amt))
            {
                nvc.Add("order.amount", Amt);
            }

            if (!String.IsNullOrEmpty(OrderCurrency))
            {
                nvc.Add("order.currency", OrderCurrency);
            }

            if (!String.IsNullOrEmpty(TransactionReference))
            {
                if (ApiOperation == "VERIFY" || ApiOperation == "PAY")
                {
                    nvc.Add("transaction.reference", TransactionId);
                }
                else
                {
                    nvc.Add("transaction.reference", TransactionReference);
                }
            }

            if (!String.IsNullOrEmpty(TransactionAmount))
            {
                nvc.Add("transaction.amount", TransactionAmount);
            }
            if (!String.IsNullOrEmpty(TransactionCurrency))
            {
                nvc.Add("transaction.currency", TransactionCurrency);
            }
            if (!String.IsNullOrEmpty(TargetTransactionId))
            {
                nvc.Add("transaction.targetTransactionId", TargetTransactionId);
            }

            if (!String.IsNullOrEmpty(SecureIdResponseUrl))
            {
                nvc.Add("3DSecure.authenticationRedirect.responseUrl", SecureIdResponseUrl);
                nvc.Add("3DSecure.authenticationRedirect.pageGenerationMode", "CUSTOMIZED");
            }
            if (!String.IsNullOrEmpty(PaymentAuthResponse))
            {
                nvc.Add("3DSecure.paRes", PaymentAuthResponse);
            }

            //browser payment
            if (!String.IsNullOrEmpty(BrowserPaymentOperation))
            {
                nvc.Add("browserPayment.operation", BrowserPaymentOperation);
            }

            //paypal
            if (!String.IsNullOrEmpty(BrowserPaymentPaymentConfirmation) && "PAYPAL" == SourceType)
            {
                nvc.Add("browserPayment.paypal.paymentConfirmation", BrowserPaymentPaymentConfirmation);
            }

            //statement Descriptor Name
            if (!String.IsNullOrEmpty(statementDescriptorName))
            {
                nvc.Add("order.statementDescriptor.name", statementDescriptorName);
            }

            if ("INITIATE_BROWSER_PAYMENT" == ApiOperation)
            {
                nvc.Add("browserPayment.returnUrl", ReturnUrl);
            }

            if(!string.IsNullOrEmpty(CustomerLastName))
            {
                nvc.Add("customer.lastName", CustomerLastName);
            }

            //build json
            Payload = JsonHelper.BuildJsonFromNVC(nvc);

            return Payload;
        } 
    }
}
