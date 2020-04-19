using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic
{
    public  class LogMessage
    {
        public static readonly string LOGIN_OK = "Successful login.";
        public static readonly string LOGIN_USER_NOT_FOUND = "Login failed. User not found.";
        public static readonly string LOGIN_WRONG_PASSWORD = "Login failed. Wrong password.";
        public static readonly string LOGIN_DATAINACCESSIBLE = "Login failed. Data inaccessible.";
        public static readonly string LOGIN_BAD_MODEL_IN = "Login failed. Bad model in.";
        public static readonly string FIXTURE_BAD_MODEL_IN = "Fixture failed. Bad model in.";
        public static readonly string FIXTURE_WRONG = "Fixture failed. Error in algorithm >>";
        public static readonly string FIXTURE_SPORT_NOT_FOUND = "Fixture failed. Sport not found.";
        public static readonly string FIXTURE_DATAINACCESSIBLE = "Fixture failed. Data inaccessible.";
        public static readonly string FIXTURE_OK = "Fixture generated.";
        public static readonly string UNIDENTIFIED = "Unidentified user";
    }
}
