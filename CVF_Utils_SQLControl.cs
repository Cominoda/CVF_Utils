//V3.0 LC 20250106 Migration vers .NET 8
//V2.7 KN 20240509 changement du login SQL
//V2.6 KN 20230804 ajout de la connection avec un SqlLogin pour localDB CV2024, a peaufiner
//V2.4 KN 20230511 connection avec un SqlLogin
//V2.3 KN 20220804 connection SQL avec autre user
//V2.1 KN 20220115 erreur d'exeption quand pas d'ouverture de connection
// V2.0 Coversion en C# - KN 29/08/2021
//armonisation du code entre les différente appli deja dévellopé
//V1.1 05/11/2021 changement de la création des paramétres sql


//using System.Data.SqlClient; //LC nuget absolete, remplacé par Microsoft.Data.SqlClient
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Principal;

namespace CVF_Utils;

public class SQLControl : IDisposable
{
    //private string Serveurname = "";
    //private string Databasename = "master";

    //private string User = "CVF";
    //private string Userpwd = "Pwd-CVF@2010";

    private SqlLogin sqlLogin = new SqlLogin();

    // Private DBCon As New SqlConnection("Server=LocalHost\Solid;Database=CXMaterials_12;User=CVF;Pwd=Pwd-CVF@2010;")
    private SqlConnection DBCon = new SqlConnection(); //"Server=" + ServeurName + ";Database=" + DatabaseName + ";User=" + User + ";Pwd=" + Userpwd + ";"
    // Private DBCON2 As New SqlConnection()
    // Private DBCon As New SqlConnection("Server=LocalHost\Solid;Database=CXMaterials_12;Trusted_Connection=yes")
    private SqlCommand DBCmd;

    // DB DATA
    public SqlDataAdapter DBDA;
    public DataTable DBDT;

    // QUERY PARAMETERS
    public List<SqlParameter> Params = new List<SqlParameter>();

    // QUERY STATISTICS
    public int RecordCount;
    public string Exception;

    // Constructeur de class -------------------------------------
    //public SQLControl(string strserveurName, bool boolAdminConnection = false)
    //{
    //    //DBCon.ConnectionString = "Server=" + Serveurname + ";Database=" + Databasename + ";User=" + User + ";Pwd=" + Userpwd + ";";

    //    sqlLogin.Serveur = strserveurName;
    //    if (boolAdminConnection)
    //    {
    //        DBCon.ConnectionString = "Server=" + strserveurName + ";Database=" + sqlLogin.Database + ";Trusted_Connection = yes";
    //        //DBCon.ConnectionString = "Server=LocalHost\\CV;Database=master;Trusted_Connection=yes";
    //    }
    //    else
    //    {
    //        DBCon.ConnectionString = "Server=" + strserveurName + ";Database=" + sqlLogin.Database + ";User=" + sqlLogin.User + ";Pwd=" + sqlLogin.Password + ";";
    //    }

    //}

    //public SQLControl(string strserveurName, string strdatabasename, bool boolAdminConnection = false)
    //{
    //    sqlLogin.Serveur = strserveurName;
    //    sqlLogin.Database = strdatabasename;

    //    if (boolAdminConnection)
    //    {
    //        DBCon.ConnectionString = "Server=" + strserveurName + ";Database=" + sqlLogin.Database + ";Trusted_Connection = yes";
    //        //DBCon.ConnectionString = "Server=localhost\\CV;Database=CVData_2021;Trusted_Connection = yes";
    //    }
    //    else
    //    {
    //        DBCon.ConnectionString = "Server=" + strserveurName + ";Database=" + strdatabasename + ";User=" + sqlLogin.User + ";Pwd=" + sqlLogin.Password + ";";
    //    }

    //    try
    //    {
    //        DBCon.Open();
    //    }
    //    catch (System.Exception ex)
    //    {
    //        //MessageBox.Show(ex.Message);
    //        Exception = "ExecQuery Error: " + Environment.NewLine + ex.Message;
    //        return;
    //    }
    //}

    //public SQLControl(string strserveurName, string strdatabasename, string user , string password)
    //{
    //          sqlLogin.Serveur = strserveurName;
    //          sqlLogin.Database = strdatabasename;
    //          sqlLogin.User = user;
    //          sqlLogin.Password = password;

    //	DBCon.ConnectionString = "Server=" + strserveurName + ";Database=" + strdatabasename + ";User=" + sqlLogin.User + ";Pwd=" + sqlLogin.Password + ";";

    //	try
    //	{
    //		DBCon.Open();
    //	}
    //	catch (System.Exception ex)
    //	{
    //		//MessageBox.Show(ex.Message);
    //		Exception = "ExecQuery Error: " + Environment.NewLine + ex.Message;
    //		return;
    //	}
    //}


    //public SQLControl(DatabaseLocalisation CV_Database)
    //      {
    //          sqlLogin.Serveur = CV_Database.ServeurName;
    //          sqlLogin.Database = CV_Database.DatabaseName;

    //          if (!string.IsNullOrEmpty(CV_Database.DatabaseUser) && !string.IsNullOrEmpty(CV_Database.DatabasePwd))
    //          {
    //              sqlLogin.User = CV_Database.DatabaseUser;
    //              sqlLogin.Password = CV_Database.DatabasePwd;
    //          }

    //          DBCon.ConnectionString = "Server=" + sqlLogin.Serveur + ";Database=" + sqlLogin.Database + ";User=" + sqlLogin.User + ";Pwd=" + sqlLogin.Password + ";";

    //          try
    //          {
    //              DBCon.Open();
    //          }
    //          catch (System.Exception ex)
    //          {
    //              MessageBox.Show(ex.Message);
    //          }
    //      }

    //Autorisation d'un constructeur null pour différé la connection SQL du conctructeur.
    public SQLControl() { }

    public SQLControl(SqlLogin login)
    {
        sqlLogin = login;
        Connection();
    }

    public SQLControl(SqlLogin login, bool boolAdminConnection)
    {
        sqlLogin = login;

        if (boolAdminConnection)
        {
            DBCon.ConnectionString = "Server=" + sqlLogin.Serveur + ";Database=" + sqlLogin.Database + ";Trusted_Connection=True;TrustServerCertificate=True;";
            //DBCon.ConnectionString = "Server=localhost\\CV;Database=CVData_2021;Trusted_Connection = yes";
        }
        else
        {
            Connection();
        }
    }

    public void SetConnection(SqlLogin login)
    {
        sqlLogin = login;
        Connection();
    }

    public void Connection()
    {
        Exception = "";
        CloseConnection();

        //string localDBPath = "S2M-C:\\Cabinet Vision\\S2M 2024\\Database\\pscn-cv.mdf";
        //string PSNCXX = @"Data Source = (localdb)\CV24; Integrated Security = SSPI ; Initial Catalog= S2M-C:\CABINET VISION\S2M 2024\Database\psnc-cv.mdf";

        //if (!string.IsNullOrEmpty(localDBPath))
        if (sqlLogin.IsLocalDB) //.Serveur.StartsWith("(localdb)"))
        {
            DBCon.ConnectionString = $@"Data Source = {sqlLogin.Serveur}; Integrated Security = SSPI ; Initial Catalog= {sqlLogin.Database}";
            // DBCon.ConnectionString = PSNCXX;
        }
        else
        {
            DBCon.ConnectionString = "Server=" + sqlLogin.Serveur + ";Database=" + sqlLogin.Database + "; TrustServerCertificate=true ;User=" + sqlLogin.User + ";Pwd=" + sqlLogin.Password + ";";
        }
        //DBCon.ConnectionString = "Server=" + Serveurname + ";Database=" + Databasename + ";User=" + User + ";Pwd=" + Userpwd + ";";

        //Console.WriteLine(DBCon.ConnectionString);
        //Console.WriteLine(PSNCXX);
        try
        {
            DBCon.Open();
        }
        catch (System.Exception ex)
        {
            //MessageBox.Show(ex.Message);
            Exception = "ExecQuery Error: " + Environment.NewLine + ex.Message;
            return;
        }

    }

    public bool ConnectionStatus()
    {
        if (DBCon.State == ConnectionState.Open)
            return true;
        else
            return false;
    }
    //-------------------------------------------------------------

    // EXECUTE QUERY SUB
    public void ExecQuery(string Query, bool Querydebug = false)
    {
        // RESET QUERY STATS
        RecordCount = 0;
        Exception = "";

        if (Querydebug)
            MessageBox.Show(Query);

        if (DBDT != null) DBDT.Clear();

        try
        {
            if (DBCon.State == ConnectionState.Closed)
            {
                try
                {
                    DBCon.Open();
                }
                catch (System.Exception ex)
                {
                    Exception = "ExecQuery Error: " + Environment.NewLine + ex.Message;
                    //MessageBox.Show(ex.Message);
                    return;
                }
            }

            // MsgBox(DBCon.ToString)

            // CREATE DB COMMAND
            DBCmd = new SqlCommand(Query, DBCon);

            // LOAD PARAMS INTO DB COMMAND
            Params.ForEach(p => DBCmd.Parameters.AddWithValue(p.ParameterName, p.Value));

            // CLEAR PARAM LIST
            Params.Clear();

            // EXECUTE COMMAND & FILL DATASET
            DBDT = new DataTable();
            DBDA = new SqlDataAdapter(DBCmd);
            RecordCount = DBDA.Fill(DBDT);

        }
        catch (System.Exception ex)
        {
            // CAPTURE ERROR
            // Call LogCréationMatériaux("ExecQuery Error: " & vbNewLine & ex.Message)
            Exception = "ExecQuery Error: " + Environment.NewLine + ex.Message;
        }
        // 
        //finally
        //{
        //}
    }

    // ADD PARAMS
    public void AddParam(string Name, object Value)
    {
        SqlParameter NewParam = new SqlParameter(Name, Value);
        Params.Add(NewParam);
    }

    // ERROR CHECKING
    public bool HasException(bool Report = false)
    {
        if (string.IsNullOrEmpty(Exception))
            return false;
        if (Report == true)
            MessageBox.Show(Exception, "Exception:", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return true;
    }

    public void CloseConnection()
    {
        try
        {
            if (DBCon.State == ConnectionState.Open)
                DBCon.Close();
        }
        catch (System.Exception)
        {
        }
    }

    public string SQLFromLocalhostODB(string tableName)
    {
        return "[ODBC;Driver={SQL Server};SERVER=" + sqlLogin.Serveur + ";DATABASE=" + sqlLogin.Database + ";UID="
            + sqlLogin.User + ";Pwd=" + sqlLogin.Password + ";TrustServerCertificate=true].[" + tableName + "]";
    }
    public string SQLFromLocaldbODB(string tableName)
    {
        //return "SELECT * Into ["+tableName+"] From [ODBC;Driver={SQL Server Native Client 11.0};" +
        //  "SERVER=(localdb)\\CV24;DATABASE=CV-C:\\CABINET VISION\\CV 2024\\Database\\report.mdf;Trusted_Connection=yes].["+ tableName + "]";
        //string database = sqlLogin.Database;

        return "[ODBC;Driver={SQL Server Native Client 11.0};" +
           $"SERVER={sqlLogin.Serveur};DATABASE={sqlLogin.Database};Trusted_Connection=yes].[{tableName}]";
    }

    public string GetServeurName() { return sqlLogin.Serveur; }
    public string GetDatabaseName() { return sqlLogin.Database; }
    public void Dispose()
    {
        try
        {
            DBCon.Close();
            DBCon.Dispose();
        }
        catch (Exception e)
        {
        }
    }
}
