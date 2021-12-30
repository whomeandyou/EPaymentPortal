/*********************************************************************************
 *       
 *                                                                               
 * This file is part of TMLM Portal project.                                 
 * Unauthorized copying of this file or any of the part is strictly prohibited.  
 * Proprietary and confidential                                                  
 *                                                                               
 * Written by Teong Wah, Feb 2019                
 *                                                                               
 *********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMLM.Common
{
    public static class ResponseReturnCode {
        /// <summary>
        /// Normally indicate the result from web api is positve.
        /// </summary>
        public const string Gen_Success = "00000";

        /// <summary>
        /// Normally indicate the result from web api is negative, 
        /// usually we could not know what the problem and required web api log for further investigation
        /// </summary>
        public const string Gen_Failed = "00001";

        /// <summary>
        /// Normally indicate no record found for some business login process
        /// usually we could not know what the problem and required web api log for further investigation
        /// </summary>
        public const string Gen_RecordNotFound = "00002";

        /// <summary>
        /// auth token pass from client was not match with the auth token in the server
        /// usually this indicate that some abnormal security case was happened
        /// </summary>
        public const string Gen_Auth_Check_Failed = "00003";

        public const string TokenExpired = "00004";

        /// <summary>
        /// database connection failed was happen at the server side
        /// </summary>
        public const string Gen_DB_Connection_Failed = "00005";

        /// <summary>
        /// unknown error happened
        /// usually we could not know what the problem and required web api log for further investigation
        /// </summary>
        public const string Gen_Error_Occur = "00006";

        /// <summary>
        /// Normally indicate the server was not reachable
        /// </summary>
        public const string Gen_ServerNotFound = "00007";

        /// <summary>
        /// Normally indicate the network was not reachable
        /// </summary>
        public const string Gen_NetworkNotReachable = "00008";

        /// <summary>
        /// Normally indicate the server internal error, we follow the http code.
        /// </summary>
        public const string Gen_AppUnknownError = "00009";

        /// <summary>
        /// Normally indicate the access to the network was forbidded.
        /// </summary>
        public const string Gen_NetworkForbidden = "00010";

        /// <summary>
        /// Normally indicate the server internal error, we follow the http code.
        /// </summary>
        public const string Gen_InternalServerError = "00011";

        //Expired Token.
        public const string AUTH_TOKEN_EXPIRED = "00012";

        //Blackedlisted Member. 
        public const string MEMBER_WAS_BLACKLISTED = "00013";

        //Token not match with Login_Id
        public const string AUTH_TOKEN_MISMATCH = "00014";

        //Member record not found by Login_Id
        public const string MEMBER_NOT_FOUND = "00015";

        public const string Timeout = "00017";

        public static class LoginReturnCode {
            public const string Login_Id_Not_Found = "00015";
            public const string Invalid_Login_Credential = "00016";
        }     

        public static class VerifyPinReturnCode {
            /// <summary>
            /// The vf pin invalid phone.
            /// </summary>
            public const string INVALID_MOBILE_NUMBER = "00031";

            /// <summary>
            /// The vf pin template not found.
            /// </summary>
            public const string TEMPLATE_NOT_FOUND = "00032";

            /// <summary>
            /// The vf pin invalid pin.
            /// </summary>
            public const string INVALID_PIN = "00033";

            /// <summary>
            /// The vf pin expired pin.
            /// </summary>
            public const string PIN_EXPIRED = "00034";

            /// <summary>
            /// The vf pin update pin failed.
            /// </summary>
            public const string UPDATED_PIN_FAILED = "00035";
        }

        public static class VerifyMemberReturnCode {
            /// <summary>
            /// Login_Id is mandatory
            /// </summary>
            public const string LOGIN_ID_REQUIRED = "00061";

            /// <summary>
            /// Email is mandatory
            /// </summary>
            public const string EMAIL_REQUIRED = "00062";

            /// <summary>
            /// Gender is mandatory
            /// </summary>
            public const string GENDER_REQUIRED = "00063";

            /// <summary>
            /// First_Name is mandatory
            /// </summary>
            public const string FIRST_NAME_REQUIRED = "00064";

            /// <summary>
            /// Last_Name is mandtory
            /// </summary>
            public const string LAST_NAME_REQUIRED = "00065";

            /// <summary>
            /// Member_Id not found
            /// </summary>
            public const string MEMBER_ID_REQUIRED = "00066";

            /// <summary>
            /// REegistration failed.
            /// </summary>
            public const string REGISTRATION_FAILED = "00067";

            /// <summary>
            /// Login_Id not found
            /// </summary>
            public const string LOGIN_ID_NOT_FOUND = "00068";

            /// <summary>
            /// Login_Id is Active
            /// </summary>
            public const string LOGIN_ID_IS_ACTIVE = "00069";

            /// <summary>
            /// Login_Id is Inactive
            /// </summary>
            public const string LOGIN_IS_INACTIVE = "00070";

            /// <summary>
            /// Invalid Password
            /// </summary>
            //public const string INVALID_USERNAME_PASSWORD = "00071";

            /// <summary>
            /// Invalid Mobile number
            /// </summary>
            public const string INVALID_MOBILE_NUMBER = "00072";

            //Mobile_No failed to update.
            public const string FAILED_TO_UPDATE_MOBILE = "00073";

            //Avatar_Img failed to update.
            public const string FAILED_TO_UPDATE_AVATAR_IMG = "00074";

            //State_Id not found
            public const string STATED_ID_NOT_FOUND = "00075";

            //Country_Id not found
            public const string COUNTRY_ID_NOT_FOUND = "00076";

            //Shipping Addr failed to update.
            public const string FAILED_TO_UPDATE_SHPPING_ADDRESS = "00077";

            //Login_Id is Disabled
            public const string MEM_LOGIN_ID_DISABLED = "00078";

            //Member email already verified
            public const string MEM_EMAIL_ALREADY_VERIFIED = "00079";

            //Member email already verified
            public const string MEM_EMAIL_YET_TO_VERIFIED = "00080";

            //Member email already Exists
            public const string MEM_EMAIL_ALREADY_EXIST = "00081";

            //Shipping Addr failed to update.
            public const string LOGIN_ID_EXIST_WITH_OTHER_TYPE = "00082";

            //Valid Login_Id
            public const string VALID_LOGIN = "00083";

        }

        public static class ChangePasswordReturnCode {
            public const string MAILER_ERROR = "00090";
        }

        public static class PolicyPaymentReturnCode
        {
            /// <summary>
            /// Invalid amount.
            /// </summary>
            public const string INVALID_AMOUNT = "00101";

            /// <summary>
            /// Invalid agent code.
            /// </summary>
            public const string INVALID_AGENT_CODE = "00102";
        }

        public static class PersonalDetailReturnCode
        {
            public const string VALIDATION_ERROR = "00400";
        }

        public static class PaymentReturnCode
        {
            public const string VALIDATION_ERROR = "00400";
            public const string DUPLICATE_TRX_NO = "00401";
        }

    }

    public enum WF_Status {
        DRAFT = 1, PENDING = 2, APPROVED = 3
    }

    public static class Sys_Config_Code {
        // db_Name
        public static string db_Name = "db_Name";

        // db_Host
        public static string db_Host = "db_Host";

        // db_User
        public static string db_User = "db_User";

        // db_Pass
        public static string db_Pass = "db_Pass";

        // sms_URL
        public static string sms_URL = "sms_URL";

        // sms_Username
        public static string sms_Username = "sms_Username";

        // sms_Password
        //public static string sms_Password = "sms_Password";

        // sms_Type
        public static string sms_Type = "sms_Type";

        // sms_Signature
        public static string sms_Signature = "sms_Signature";

        // sms_Service_Id
        public static string sms_Service_Id = "sms_Service_Id";

        // sms_Exp_Interval
        public static string sms_Exp_Interval = "sms_Exp_Interval";

        // sms_Template_Id_REG
        public static string sms_Template_Id_REG = "sms_Template_Id_REG";

        // sms_Service_On
        public static string sms_Service_On = "sms_Service_On";

        // Host
        public static string Host = "Host";

        // SMTPAuth
        public static string SMTPAuth = "SMTPAuth";

        // SMTP_User
        public static string SMTP_User = "SMTP_User";

        // SMTP_PW
        public static string SMTP_PW = "SMTP_PW";

        // SMTP_Secure
        public static string SMTP_Secure = "SMTP_Secure";

        // SMTP_Port
        public static string SMTP_Port = "SMTP_Port";

        // From_Email
        public static string From_Email = "From_Email";

        // From_Name
        public static string From_Name = "From_Name";

        // ReplyTo_Email
        public static string ReplyTo_Email = "ReplyTo_Email";

        // ReplyTo_Name
        public static string ReplyTo_Name = "ReplyTo_Name";

        // DateTimeZone
        public static string DateTimeZone = "DateTimeZone";

        // DB_Date_Format
        public static string DB_Date_Format = "DB_Date_Format";

        // Enable_Debugger_Log
        public static string Enable_Debugger_Log = "Enable_Debugger_Log";

        // Avatar_Img_URL
        public static string Avatar_Img_URL = "Avatar_Img_URL";

        // 
        public static string Image_Server = "Image_Server";

        // 
        public static string FTP_User = "FTP_User";

        // 
        public static string FTP_Pass = "FTP_Pass";

        // 
        public static string FTP_Server = "FTP_Server";

        // 
        //public static string PasswordSaltKey = "PasswordSaltKey";
    }
}
