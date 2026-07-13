//4.1 LC 20250514 Update pour CV2025
//4.0 LC 20250111 Migration vers .NET 8
//3.4 KN 20240916 Ajout des infos pour faire une requete sur le psnc-CV
//3.3 KN 20240520 Migration pour CV2024;
//3.2; KN 20240509 meilleur gestion des login SQL;
//3.1; Set Langue pour Report 
//V3.0; 20230515 meilleur gestion d'ecxeption
//V2.9; sauvegarde du status utilisation réseau ou monoposte.
//V2.82; 20230403 suite des modifs du chemin pour Common2023 (améliration de la récupération des chemin de database)
//V2.8 ; 20230317 ajout clé pour CV2023 dans Common 2023
//V2.7 ; 20220221 correction erreur clé registre pour CV11 et CV12 et stockage Databaseuse 																   
//V2.6 ; 20220216 Merge avec la version de CVtoERP. 
//V2.5 ; 20220115 Changement des variable servername et databasenem en get public; ortographe ; getsolidgraphique
//V2.4 ; 20211210 Update pour CV2022
//V2.4 ; Ajout d'une option de dérogation pour la localisation - 2021/11/26
//V2.3 ; Ajout de variable pour la recherche de fichier de database
//V2.2 Modification pour les linkCommand 
//V2.1 Modification des avertissement (CS0168)
// V2.0 Coversion en C# - KN 29/08/2021
// Database V1.1 'KN 05/04/2021
// gestion erreur si pas de cabinet vision installé


//using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;   // outil regisre

namespace CVF_Utils;

public enum Logiciel
{
    Common,
    Report,
    PsncXX
}

public class DatabaseLocalisation
{

    private const string REG_VERSION = "XXX";
    private string Version;
    private bool DatabaseInCommon = false;
    private bool isNetwork = false;

    private bool DatabaseLocalisation_DEBUG;

    private string REG_CURRENT_USER; // = "HKEY_CURRENT_USER\Software\Cabinet Vision\Common\CurrentUser"                        'CurrentUser from registry
                                     //private RegistryKey REG_CURRENT_USER;

    private string REG_COMMON_32;
    private string REG_COMMON_64;

    private string REG_S2M_PATH_32; // = "HKEY_LOCAL_MACHINE\Software\Cabinet Vision\NcCenter_XXX\DBasePath"
    private string REG_S2M_PATH_64; // = "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Cabinet Vision\NcCenter_XXX\DBasePath"

    private string REG_CV_PATH_32; // = "HKEY_LOCAL_MACHINE\Software\Cabinet Vision\Solid_XXX\Settings\DBasePath"
    private string REG_CV_PATH_64; // = "HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Cabinet Vision\Solid_XXX\Settings\DBasePath"
    private string REG_SQL_DATABASE;
    private string REG_LOCALDB; // HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\Common 2024

    //public string ServeurName { get; private set; } // = "WIN-UI364V0D1II\Solid"
    //public string DatabaseName { get; private set; }  // = "CXMaterials_12"

    public string psncfilename { get; private set; }
    public string reportfilename { get; private set; }

    public SqlLogin sqlLogin { get; private set; } = new SqlLogin();
    public Dictionary<Logiciel,SqlLogin?> logins { get; private set; } = new Dictionary<Logiciel, SqlLogin?>();

    //public string Database { get; set; } = "master";

    //public string User { get; set; } = "CVF";
    //public string Password { get; set; } = "Pwd-CVF@2010";

    //      public string DatabaseUser { get; set; } = "";
    //public string DatabasePwd { get; set; } = "";

    // private string REG_UserCVsettings;
    private RegistryKey REG_UserCVsettings;

    private string Network { get; set; } 
    private string Path_PSNC;
    private string Path_Report;
    private string PathFile_PSNC;
    private string PathFile_Report;
    private string Path_S2M;
    private string Path_CV;
    private string Database_Path;
    private string CleGraph_Path;
    private string _setlocalisaionderogation;
    private string Exception;
    private string localDBInstance;

    //Constructeur de class
    public DatabaseLocalisation(bool boolScript_Debug = false)
    {
        DatabaseLocalisation_DEBUG = boolScript_Debug;
    }

    public DatabaseLocalisation(string strVersion, bool boolScript_Debug = false)
    {
        DatabaseLocalisation_DEBUG = boolScript_Debug;
        SetVersion(strVersion);

    }
    // -----------------------------------------------------

    private void Cle_Registre()
    {
        REG_LOCALDB = "";

        if (Version == "11" | Version == "12")
        {
            REG_CURRENT_USER = @"HKEY_CURRENT_USER\SOFTWARE\Cabinet Vision\Common";     // CurrentUser from registry

            REG_COMMON_32 = @"HKEY_LOCAL_MACHINE\Software\Cabinet Vision\Common";
            REG_COMMON_64 = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Cabinet Vision\Common";

            REG_S2M_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Cabinet Vision\NcCenter_XXX", Version);             // DBasePath
            REG_S2M_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Cabinet Vision\NcCenter_XXX", Version);

            REG_CV_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Cabinet Vision\Solid_XXX\Settings", Version); // DBasePath
            REG_CV_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Cabinet Vision\Solid_XXX\Settings", Version);

            //REG_UserCVsettings = RegistryVersion(@"Software\Cabinet Vision\Solid_XXX\Settings", Version); //la partie current usert est définie lors de la création de la clé
            REG_UserCVsettings = Registry.CurrentUser.OpenSubKey(RegistryVersion(@"Software\Cabinet Vision\Solid_XXX\Settings", Version));

            REG_SQL_DATABASE = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\CABINET VISION\Solid_XXX\Settings\Material", Version);

            psncfilename = "PSNC-CV.mdb";
            reportfilename = "Report.mdb";
        }
        else if (Version == "2021")
        {
            REG_CURRENT_USER = @"HKEY_CURRENT_USER\SOFTWARE\Hexagon\CABINET VISION\Common";     // CurrentUser from registry

            REG_COMMON_32 = @"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\Common";
            REG_COMMON_64 = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\CABINET VISION\Common";

            REG_S2M_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\S2M XXX", Version);      // DBasePath
            REG_S2M_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\S2M XXX", Version); // @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\S2M XXX";

            REG_CV_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version); // DBasePath
            REG_CV_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\CV XXX\Settings", Version); //@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\CV XXX\Settings";

            //REG_UserCVsettings = RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version);  //la partie current usert est définie lors de la création de la clé
            REG_UserCVsettings = Registry.CurrentUser.OpenSubKey(RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version), true);

            REG_SQL_DATABASE = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\CABINET VISION\CV XXX\Settings\CVData", Version);

            psncfilename = "PSNC-CV.mdb";
            reportfilename = "Report.mdb";
        }
        else if (Version == "2022")
        {
            REG_CURRENT_USER = RegistryVersion(@"HKEY_CURRENT_USER\SOFTWARE\Hexagon\CABINET VISION\Common XXX", Version);     // CurrentUser from registry

            REG_COMMON_32 = ""; //RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\Common", Version);
            REG_COMMON_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\Common XXX", Version);

            //REG_S2M_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\S2M XXX", Version);      // DBasePath
            REG_S2M_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\S2M XXX", Version); // @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\S2M XXX";

            //REG_CV_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version); // DBasePath
            REG_CV_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\CV XXX\Settings", Version); //@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\CV XXX\Settings";

            //REG_UserCVsettings = RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version);  //la partie current usert est définie lors de la création de la clé
            REG_UserCVsettings = Registry.CurrentUser.OpenSubKey(RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version), true);

            REG_SQL_DATABASE = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\CV XXX\Settings\CVData", Version);

            psncfilename = "PSNC-CV.accdb";
            reportfilename = "Report.accdb";
        }
        else if (Version == "2023")
        {
            REG_CURRENT_USER = RegistryVersion(@"HKEY_CURRENT_USER\SOFTWARE\Hexagon\CABINET VISION\Common XXX", Version);     // CurrentUser from registry

            REG_COMMON_32 = ""; //RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\Common", Version);
            REG_COMMON_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\Common XXX", Version);

            //REG_S2M_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\S2M XXX", Version);      // DBasePath
            REG_S2M_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\S2M XXX", Version); // @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\S2M XXX";

            //REG_CV_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version); // DBasePath
            //Pour la 2023 on a besoin de dédouber entre Common 2023 et CV2023; CVDATA.mdf et graphique sont dans Common mais report dans cv database
            REG_CV_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\CV XXX\Settings", Version); //@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\CV XXX\Settings";

            //REG_UserCVsettings = RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version);  //la partie current usert est définie lors de la création de la clé
            REG_UserCVsettings = Registry.CurrentUser.OpenSubKey(RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version), true);

            REG_SQL_DATABASE = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\Common XXX\CVData", Version);

            psncfilename = "PSNC-CV.accdb";
            reportfilename = "Report.accdb";

            DatabaseInCommon = true;
        }
        else //2024 and 2025
        {
            REG_CURRENT_USER = RegistryVersion(@"HKEY_CURRENT_USER\SOFTWARE\Hexagon\CABINET VISION\Common XXX", Version);     // CurrentUser from registry

            REG_COMMON_32 = ""; //RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\Common", Version);
            REG_COMMON_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\Common XXX", Version);

            //REG_S2M_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\S2M XXX", Version);      // DBasePath
            REG_S2M_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\S2M XXX", Version); // @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Hexagon\Cabinet Vision\S2M XXX";

            //REG_CV_PATH_32 = RegistryVersion(@"HKEY_LOCAL_MACHINE\Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version); // DBasePath
            //Pour la 2023 on a besoin de dédoubler entre Common 2023 et CV2023; CVDATA.mdf et graphique sont dans Common mais report dans cv database
            REG_CV_PATH_64 = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\CV XXX\Settings", Version);

            //REG_UserCVsettings = RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version);  //la partie current usert est définie lors de la création de la clé
            REG_UserCVsettings = Registry.CurrentUser.OpenSubKey(RegistryVersion(@"Software\Hexagon\Cabinet Vision\CV XXX\Settings", Version), true);

            REG_SQL_DATABASE = RegistryVersion(@"HKEY_LOCAL_MACHINE\SOFTWARE\Hexagon\CABINET VISION\Common XXX\CVData", Version);

            REG_LOCALDB = $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Hexagon\\CABINET VISION\\Common {Version}";

            psncfilename = "PSNC-CV.mdf";
            reportfilename = "Report.mdf";

            DatabaseInCommon = true;
        }
    }
    // ---------------------------------------------------------------------------------------------------

    public bool SetVersion(string strVersion) // Report_or_Psnc As String, 
    {
        // mise a zero des variables  
        Exception = "";
        Path_S2M = "";
        Path_CV = "";
        Path_PSNC = "";
        Path_Report = "";
        PathFile_PSNC = "";
        PathFile_Report = "";
        sqlLogin.Serveur = "";
        sqlLogin.Database = "";
        Database_Path = "";
        localDBInstance = "";

        //Login pour la DB CV par default.
        sqlLogin.User = "CVF";
        sqlLogin.Password = "Pwd-CVF@2010";   

        //
        Version = strVersion;
        Cle_Registre();

        string sPath;
        string sUser;
        //string Network;

        logins.Clear();

        try
        {
            Network = (string)Registry.GetValue(REG_COMMON_64, "NetRights", "0");
            if (Network == "1") isNetwork = true;
        }
        catch (Exception)
        {
            try
            {
                Network = (string)Registry.GetValue(REG_COMMON_32, "NetRights", "0");
                if (Network == "1") isNetwork = true;
            }
            catch (Exception)
            {
                isNetwork = false;
                Network = "0";
                MessageBox.Show("Erreur dans SetNetWork impossible de lire la clé NetRights");
            }
        }

        Path_S2M = GetPSNCPath();
        Path_CV = GetREPORTPath();

        if (isNetwork)
        {
            try
            {
                sUser = GetCurrentUser();
                sPath = System.IO.Path.Combine(GetUserPath(), sUser);

                PathFile_PSNC = System.IO.Path.Combine(sPath, psncfilename);
                PathFile_Report = System.IO.Path.Combine(sPath, reportfilename);

                //Path_S2M = sPath;
                //Path_CV = sPath;
                Path_PSNC = sPath;
                Path_Report = sPath;

                if (DatabaseInCommon)
                {
                    Database_Path = (string)Registry.GetValue(REG_SQL_DATABASE, "CVDataLocalDBPath", "0");
                    CleGraph_Path = (string)Registry.GetValue(REG_SQL_DATABASE, "GraphicsPath", "0");
                }
                else
                {
                    Database_Path = (string)Registry.GetValue(REG_CV_PATH_64, "DBasePath", "0");
                    CleGraph_Path = (string)Registry.GetValue(REG_CV_PATH_64, "GraphicsPath", "0");  //System.IO.Path.Combine(Path_CV, "Graphics");
                }
            }
            catch (Exception ex)
            {
                Exception = "Database Netwok Error: " + Environment.NewLine + ex.Message;
                return false; // "Pas de database cabinet vision : Network :" + Network + (char)10 + ex.Message;
            }
        }
        else
            try
            {
                Path_PSNC = Path_S2M;
                Path_Report = Path_CV;
                PathFile_PSNC = System.IO.Path.Combine(Path_S2M, psncfilename);
                PathFile_Report = System.IO.Path.Combine(Path_CV, reportfilename);

                if (DatabaseInCommon)
                {
                    Database_Path = (string)Registry.GetValue(REG_SQL_DATABASE, "CVDataLocalDBPath", "0");
                    CleGraph_Path = (string)Registry.GetValue(REG_SQL_DATABASE, "GraphicsPath", "0");
                }
                else
                {
                    Database_Path = (string)Registry.GetValue(REG_CV_PATH_64, "DBasePath", "0");
                    CleGraph_Path = (string)Registry.GetValue(REG_CV_PATH_64, "GraphicsPath", "0");  //System.IO.Path.Combine(Path_CV, "Graphics");
                }

            }
            catch (Exception ex)
            {
                Exception = "Database Error: " + Environment.NewLine + ex.Message;
                return false; // "Pas de database cabinet vision : Network :" + Network + (char)10 + ex.Message;
            }

        sqlLogin.Serveur = GetServeurName();
        sqlLogin.Database = GetDatabaseName();

        if (LocalisaionDerogation != null & LocalisaionDerogation != "")
        {

            Path_PSNC = LocalisaionDerogation;
            Path_Report = LocalisaionDerogation;
            PathFile_PSNC = System.IO.Path.Combine(LocalisaionDerogation, psncfilename);
            PathFile_Report = System.IO.Path.Combine(LocalisaionDerogation, reportfilename);

        }

        localDBInstance = LocalDBInstance();

        logins[Logiciel.Common] = sqlLogin;
        if(Convert.ToInt16(Version) >= 2024)
        {
            logins[Logiciel.Report] = GetDBInfo(Logiciel.Report);
            logins[Logiciel.PsncXX] = GetDBInfo(Logiciel.PsncXX);
            logins[Logiciel.Report].IsLocalDB = true;
            logins[Logiciel.PsncXX].IsLocalDB = true;
        }
        //else
        //{
        //    logins[Logiciel.Report] = null;
        //    logins[Logiciel.PsncXX] = null;
        //}
        return true;
    }

    public string LocalisaionDerogation
    {
        //vérifier méthode pour CV2024
        get
        {
            return _setlocalisaionderogation;
        }
        set
        {
            _setlocalisaionderogation = value;
            Path_PSNC = _setlocalisaionderogation;
            Path_Report = _setlocalisaionderogation;
            PathFile_PSNC = System.IO.Path.Combine(_setlocalisaionderogation, psncfilename);
            PathFile_Report = System.IO.Path.Combine(_setlocalisaionderogation, reportfilename);
        }
    }
    //Return de valeur-------------------
    public string GetVersion() => Version;

    public string GetPsnc_PathFile() => PathFile_PSNC;

    public string GetReport_PathFile() => PathFile_Report;

    public string GetCV_localisation() => Path_CV;

    public string GetS2M_localisation() => Path_S2M;

    public string GetReport_Path() => Path_Report;

    public string GetPsnc_Path() => Path_PSNC;

    public string GetDatabase_Path() => Database_Path;

    //public SqlLogin GetSqlLogin() { return sqlLogin; }

    public string GetCV_localisationGraphiq() => CleGraph_Path;

    //public string GetLocalDBInstance() => localDBInstance;

    //public string GetLocalDBName(Logiciel logiciel)
    //{
    //    if (logiciel == Logiciel.CabinetVision)
    //        return $"CV-{PathFile_Report}";
    //    else if (logiciel == Logiciel.S2M)
    //        return $"S2M-{PathFile_PSNC}";

    //    return "";
    //}

    //public SqlLogin GetDBInfo(Logiciel logiciel)
    //{
    //    SqlLogin con = new SqlLogin();

    //    con.Serveur = localDBInstance;

    //    if (logiciel == Logiciel.Report)
    //        con.Database = $"CV-{PathFile_Report}";
    //    else if (logiciel == Logiciel.PsncXX)
    //        con.Database = $"S2M-{PathFile_PSNC}";
    //    else if (logiciel == Logiciel.Common)
    //    {
    //        con.Serveur = sqlLogin.Serveur;
    //        con.Database = sqlLogin.Database;
    //        con.User = sqlLogin.User;
    //        con.Password = sqlLogin.Password;
    //    }

    //    return con;
    //}
    public SqlLogin GetDBInfo(Logiciel logiciel)
    {
        SqlLogin con = new SqlLogin();
        string selectedPath = "";



        //if (logiciel == Logiciel.Report) {
        //    con.Database = $"CV-{PathFile_Report}";
        //    selectedPath = PathFile_Report; }
        //else if (logiciel == Logiciel.PsncXX) { 
        //    con.Database = $"S2M-{PathFile_PSNC}";
        //    selectedPath = PathFile_PSNC; }
        if (logiciel == Logiciel.Common)
        {
            con.Serveur = sqlLogin.Serveur;
            con.Database = sqlLogin.Database;
            con.User = sqlLogin.User;
            con.Password = sqlLogin.Password;
        }
        else
        {
            if (logiciel == Logiciel.Report)
            {
                //con.Database = $"CV-{PathFile_Report}";
                selectedPath = PathFile_Report;
            }
            else if (logiciel == Logiciel.PsncXX)
            {
                //con.Database = $"S2M-{PathFile_PSNC}";
                selectedPath = PathFile_PSNC;
            }
            con.Serveur = localDBInstance;

            SQLControl sql = new SQLControl(new SqlLogin() { Serveur = con.Serveur });
            sql.ExecQuery("SELECT name FROM master.sys.databases");

            if (sql.HasException(true))
            {
                MessageBox.Show("Erreur de connexion à la base de données : " + sql.Exception);
                return new SqlLogin();
            }

            // Vérifier si la base de données existe
            bool databaseExists = false;
            for (int i = 0; i < sql.DBDT.Rows.Count; i++)
            {
                string dbname = sql.DBDT.Rows[i]["name"].ToString().ToLower();
                if (dbname.Contains(selectedPath.ToLower()))
                {
                    con.Database = dbname;
                    databaseExists = true;
                    break;
                }
                //else if (dbname == selectedPath.ToLower())
                //{
                //    databaseExists = true;
                //    con.Database = dbname;
                //    return con;
                //}
            }
            if (!databaseExists)
            {
                MessageBox.Show($"La base de données '{selectedPath}' n'existe pas sur le serveur '{con.Serveur}'.");
                return new SqlLogin();
            }


        }


        return con;
    }

    //Set des nouvelle valeur de database ------------------------------
    public void SetS2M_localisation(string strPath)
    {

        if (strPath == "" || strPath == null)
        {
            return;
        }
        else
        {
            string strCleName = "DBasePath";

            try
            {
                Registry.SetValue(REG_S2M_PATH_64, strCleName, strPath);
            }
            catch (Exception)
            {
                try
                {
                    Registry.SetValue(REG_S2M_PATH_32, strCleName, strPath);
                }
                catch (Exception)
                {
                    MessageBox.Show("Impossible de définir la clé registre SetS2M_localisation");
                    return;
                }
            }
        }

        SetVersion(Version); // réinitialisation des chemins de base pour la class
    }

    public void SetSolid_localisation(string strPath)
    {
        if (strPath == "" || strPath == null)
        {
            return;
        }
        else
        {
            string strCleName = "DBasePath";
            string strCleGraph = "GraphicsPath";

            try
            {
                if (DatabaseInCommon)
                {
                    strCleName = "CVDataLocalDBPath";
                    strCleGraph = "GraphicsPath";

                    Registry.SetValue(REG_SQL_DATABASE, strCleName, strPath + "\\");
                    Registry.SetValue(REG_SQL_DATABASE, strCleGraph, strPath + "\\Graphics\\");

                }
                else
                {
                    Registry.SetValue(REG_CV_PATH_64, strCleName, strPath + "\\");
                    Registry.SetValue(REG_CV_PATH_64, strCleGraph, strPath + "\\Graphics\\");
                }

            }
            catch (Exception)
            {
                if (DatabaseInCommon)
                {
                    MessageBox.Show("Impossible de définir la clé registre");
                    return;
                }
                else
                {
                    try
                    {
                        Registry.SetValue(REG_CV_PATH_32, strCleName, strPath + "\\");
                        Registry.SetValue(REG_CV_PATH_32, strCleGraph, strPath + "\\Graphics\\");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Impossible de définir la clé registre");
                        return;
                    }
                }
            }
        }

        Attach_BD(strPath);

        SetVersion(Version); // réinitialisation des chemins de base pour la class
    }


    // Fonction interne pour chercher la localisation --------------------------------------------
    private string GetCurrentPath(string Report_or_Psnc)
    {
        string GetCurrentPath = "";
        string sPath;
        string sUser;
        //string Network;

        try
        {
            Network = (string)Registry.GetValue(REG_COMMON_64, "NetRights", "0");
            if (Network == "1") isNetwork = true;
        }
        catch (Exception)
        {
            try
            {
                Network = (string)Registry.GetValue(REG_COMMON_32, "NetRights", "0");
                if (Network == "1") isNetwork = true;
            }
            catch (Exception)
            {
                isNetwork = false;
                Network = "0";
                MessageBox.Show("Erreur dans SetNetWork impossible de lire la clé NetRights");
            }
        }

        if (isNetwork)
        {
            sUser = GetCurrentUser();
            sPath = System.IO.Path.Combine(GetUserPath(), sUser);
        }
        else if (Report_or_Psnc == "psnc-cv.mdb")
            sPath = GetPSNCPath();
        else
            sPath = GetREPORTPath();

        GetCurrentPath = System.IO.Path.Combine(sPath, Report_or_Psnc);

        return GetCurrentPath;
    }

    private string GetPSNCPath()
    {
        string GetPSNCPath = "";
        try
        {
            GetPSNCPath = (string)Registry.GetValue(REG_S2M_PATH_64, "DBasePath", "toto");
        }
        catch (Exception)
        {
            try
            {
                GetPSNCPath = (string)Registry.GetValue(REG_S2M_PATH_32, "DBasePath", "");
            }
            catch (Exception)
            {
                MessageBox.Show("Erreur dans GetPSNCPath impossible de lire la clé DBasePath");
                GetPSNCPath = "";
            }
        }
        return GetPSNCPath;
    }

    // GetReportPath
    private string GetREPORTPath()
    {
        string GetREPORTPath = "";
        try
        {
            GetREPORTPath = (string)Registry.GetValue(REG_CV_PATH_64, "DBasePath", "");
        }
        catch (Exception)
        {
            try
            {
                GetREPORTPath = (string)Registry.GetValue(REG_CV_PATH_32, "DBasePath", "");
            }
            catch (Exception)
            {
                MessageBox.Show("Erreur dans GetREPORTPath impossible de lire la clé DBasePath");
                GetREPORTPath = "";
            }
        }
        return GetREPORTPath;
    }

    // ------------------------------------------------------------------------------
    // GetCurrentUser
    // Reads the current user from the registry
    private String GetCurrentUser()
    {
        string GetCurrentUser = "";
        try
        {
            GetCurrentUser = (string)Registry.GetValue(REG_CURRENT_USER, "CurrentUser", "Administrateur");
        }
        catch (Exception ex)
        {
            GetCurrentUser = "Erreur User" + (char)10 + ex.Message;
        }
        return GetCurrentUser;
    }

    // ------------------------------------------------------------------------------
    // GetUserPath
    // Reads User Path from registry
    private string GetUserPath()
    {
        string strGetUserPath = "";
        try
        {
            strGetUserPath = (string)Registry.GetValue(REG_COMMON_64, "UserPath", "");
        }
        catch (Exception)
        {
            try
            {
                strGetUserPath = (string)Registry.GetValue(REG_COMMON_32, "UserPath", "");
            }
            catch (Exception)
            {
                MessageBox.Show("Erreur dans GetUserPath impossible de lire la clé UserPath");
                strGetUserPath = "";
            }

        }
        return strGetUserPath;
    }
    private string GetServeurName()
    {

        string CleServeurName = "CVDataSQLPath";
        string CleServeurName_defvalue = @"Localhost\CVData_" + Version;

        if (Version == "11" || Version == "12")
        {
            CleServeurName = "XMaterialSQLPath";
            CleServeurName_defvalue = @"Localhost\CXMaterials_" + Version;

        }

        string strservername = (string)Registry.GetValue(REG_SQL_DATABASE, CleServeurName, CleServeurName_defvalue);
        sqlLogin.Serveur = strservername;
        return strservername;
    }

    private string GetDatabaseName()
    {
        string CleRegistre = "CVDataCatalog";
        string defaultvaleur = "CvData_" + Version;

        if (Version == "11" || Version == "12")
        {
            CleRegistre = "CxMaterialsCatalog";
            defaultvaleur = "CxMaterials_" + Version;
        }

        string strDatabaseName = (string)Registry.GetValue(REG_SQL_DATABASE, CleRegistre, defaultvaleur);
        sqlLogin.Database = strDatabaseName;
        return strDatabaseName;
    }

    private string LocalDBInstance()
    {
        localDBInstance = string.Empty;

        if (Convert.ToInt16(Version) >= 2024)
        {
            localDBInstance = (string)Registry.GetValue(REG_LOCALDB, "localDBInstance", "0");
        }

        return localDBInstance;
    }

    // ------------------------------------------------------------------------------
    // Registry Version
    // ------------------------------------------------------------------------------
    private string RegistryVersion(string sRegistryKey, string sVersion)
    {
        //RegistryVersion = Strings.Replace(sRegistryKey, REG_VERSION, sVersion);
        string strRegistre = sRegistryKey;
        return strRegistre.Replace(REG_VERSION, sVersion);
    }

    //méthode pour nouvelle base de donnée SQL
    private void Attach_BD(string DBasePath)
    {
        if (DBasePath.Length < 1) return;

        //string CleMDFfileName = "CVDataLocalDBTitle";//
        string MDFfileName = "CVData.mdf"; //CVDataLocalDBTitle
        string LDFfileName = "CVData_log.ldf";

        if (Version == "11" || Version == "12")
        {
            //CleMDFfileName = "XMaterialLocalDBTitle";
            MDFfileName = "CxMaterials.mdf";
            LDFfileName = "CxMaterials_log.ldf";
        }

        SQLControl CVDatabase = new SQLControl(sqlLogin, true);

        if (sqlLogin.Serveur.Length > 0)
        {
            string strPath_mdf = "";
            string strPath_ldf = "";

            if (sqlLogin.Database.Length > 0)
            {
                // on détache la database
                CVDatabase.ExecQuery("EXEC sp_detach_db '" + sqlLogin.Database + "'");
                if (CVDatabase.HasException(true)) return;
            }

            strPath_mdf = Path.Combine(DBasePath, MDFfileName);
            strPath_ldf = Path.Combine(DBasePath, LDFfileName);

            if (strPath_ldf.Length > 0)
            {
                // on Attache une nouvelle base
                CVDatabase.ExecQuery("EXEC sp_attach_db '" + sqlLogin.Database + "', '" + strPath_mdf + "', '" + strPath_ldf + "'");
                if (CVDatabase.HasException(true)) return;

                if (Version == "12")
                {

                    string strdatabasename = (string)Registry.GetValue(REG_SQL_DATABASE, "XConstructionSQLPath", "CXConstruction_12");
                    MDFfileName = "CXConstruction.mdf";
                    LDFfileName = "CXConstruction_log.ldf";

                    if (sqlLogin.Database.Length > 0) strPath_mdf = Path.Combine(DBasePath + MDFfileName);
                    if (strPath_mdf.Length > 0) strPath_ldf = Path.Combine(DBasePath + LDFfileName);

                    CVDatabase.ExecQuery("EXEC sp_attach_db '" + strdatabasename + "', '" + strPath_mdf + "', '" + strPath_ldf + "'"); // On Attache une nouvelle base CXConstruction pour CV12
                    if (CVDatabase.HasException(true)) return;
                }
            }
        }
    }

    public void SetLangueValue(string strCleValue)
    {
        string strCle = "HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION";

        if (Version == "11" || Version == "12")
        {
            strCle = "HKEY_CURRENT_USER\\Software\\CABINET VISION";
        }

        Registry.SetValue(strCle, "Language", strCleValue);
    }

    public void SetLangueReportValue(string strCleValue)
    {

        if (Convert.ToInt16(Version) < 2023) //pas de rapport langue avant 2023 relié au language
            return;

        //string strCle = "HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION";
        string strCle = "HKEY_LOCAL_MACHINE\\Software\\Hexagon\\CABINET VISION";
        Registry.SetValue(strCle, "LanguageSystemReports", strCleValue);

        strCle = "HKEY_CURRENT_USER\\Software\\Hexagon\\CABINET VISION";
        Registry.SetValue(strCle, "LanguageSystemReports", strCleValue);
    }

    public void SetLaunchList(int strCleValue)
    {
        //RegistryKey KeyName = Registry.CurrentUser.OpenSubKey(REG_UserCVsettings);
        REG_UserCVsettings.SetValue("LaunchList", strCleValue, RegistryValueKind.DWord);
    }

    public bool GetLaunchList()
    {
        int launchlist;
        try
        {
            launchlist = (int)REG_UserCVsettings.GetValue("LaunchList");
        }
        catch
        {
            return false;
        }

        if (launchlist == 1)
            return true;
        else
            return false;

    }

    public void SetNetWork(string strCleValue)
    {
        string CV_network;

        try
        {
            CV_network = (string)Registry.GetValue(REG_COMMON_64, "NetRights", "0");
            Registry.SetValue(REG_COMMON_64, "NetRights", strCleValue);
        }
        catch (Exception)
        {
            try
            {
                CV_network = (string)Registry.GetValue(REG_COMMON_32, "NetRights", "1");
                Registry.SetValue(REG_COMMON_32, "NetRights", strCleValue);
            }
            catch (Exception)
            {
                MessageBox.Show("Erreur dans SetNetWork impossible de lire la clé NetRights");
                return;
            }
        }
        Network = strCleValue;
    }

    public string GetNetWork()
    {
        return Network;
    }
    public bool IsNetwork()
    {
        return isNetwork;
    }

    public string GetProgramPath(string Logiciel)
    {
        string strProgramPath = "";
        try
        {
            if (Logiciel == "CV" || Logiciel == "Solid")
            {
                strProgramPath = (string)Registry.GetValue(REG_CV_PATH_64, "ProgramPath", "");
            }
            else
            {
                strProgramPath = (string)Registry.GetValue(REG_S2M_PATH_64, "ProgramPath", "");
            }

        }
        catch (Exception)
        {
            try
            {
                if (Logiciel == "CV" || Logiciel == "Solid")
                {
                    strProgramPath = (string)Registry.GetValue(REG_CV_PATH_32, "ProgramPath", "");
                }
                else
                {
                    strProgramPath = (string)Registry.GetValue(REG_S2M_PATH_32, "ProgramPath", "");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Erreur dans GetProgramPath impossible de lire la clé ProgramPath");
                strProgramPath = "";
            }
        }
        return strProgramPath;
    }

    //-----------------------------------------------------
    public LinkCommand ReadeLinkCommand(string LinkCommand)
    {
        LinkCommand lstLinkCommand = new LinkCommand();

        for (var Index = 1; Index <= 5; Index++)
        {
            string strlinkcommand = "";
            string strlinkcommand_value = "";
            string strlinkcommandlabel = "";
            string strlinkLabel_value = "";

            if (Index == 1)
            {
                strlinkcommand = LinkCommand;
                strlinkcommandlabel = LinkCommand.Replace("Command", "Label");
            }
            else
            {
                strlinkcommand = LinkCommand + Convert.ToString(Index, System.Globalization.CultureInfo.InvariantCulture);
                strlinkcommandlabel = LinkCommand.Replace("Command", "Label") + Convert.ToString(Index, System.Globalization.CultureInfo.InvariantCulture);
            }

            //CleRegistre = RegistryVersion(REG_UserCVsettings, Version);
            strlinkcommand_value = (String)REG_UserCVsettings.GetValue(strlinkcommand, "");
            strlinkLabel_value = (String)REG_UserCVsettings.GetValue(strlinkcommandlabel, "Transmettre à l'usine");

            if (strlinkcommand_value != "")
            {
                lstLinkCommand.lst.Add(new LinkCommand() { LinkIndex = Index, LinkName = strlinkLabel_value, LinkFile = strlinkcommand_value });
            }
        }
        return lstLinkCommand;
    }

    public void CreateLinkCommande(string LinkCommand, string LinkCommand_Value, string LinkCommand_Name = "", bool AvertissementCreataion = true)
    {
        //
        string strlinkcommand_value = "";
        string strlinkcommand = "";
        string strlinkcommandlabel = "";

        for (int Index = 1; Index <= 5; Index++)
        {
            if (Index == 1)
            {
                strlinkcommand = LinkCommand;
                if (LinkCommand_Name != "") { strlinkcommandlabel = LinkCommand.Replace("Command", "Label"); }
            }
            else
            {
                strlinkcommand = LinkCommand + Convert.ToString(Index, System.Globalization.CultureInfo.InvariantCulture);
                if (LinkCommand_Name != "") { strlinkcommandlabel = LinkCommand.Replace("Command", "Label") + Convert.ToString(Index, System.Globalization.CultureInfo.InvariantCulture); }
            }

            strlinkcommand_value = (String)REG_UserCVsettings.GetValue(strlinkcommand, "");

            if (strlinkcommand_value == "")
            {
                REG_UserCVsettings.SetValue(strlinkcommand, LinkCommand_Value);
                if (LinkCommand_Name != "") { REG_UserCVsettings.SetValue(strlinkcommandlabel, LinkCommand_Name); }
                if (AvertissementCreataion)
                    MessageBox.Show("Bouton créé avec succés");
                return;
            }
        }
        MessageBox.Show("Impossible de créer le bouton vous avez ateint le nombre maxi autorisé.");
    }
    public void CreateCNCPOPULATEREPORT()
    {
        REG_UserCVsettings.SetValue("CNCPOPULATEREPORT", 1, RegistryValueKind.DWord);
    }

    public void DeleteLinkCommand(int Index, string LinkCommand_Name)
    {
        string strlinkcommand_num = "";
        string strlinkcommand_label = "";
        //string strlinkcommand_Image = "";
        if (Index == 1)
        {
            strlinkcommand_num = LinkCommand_Name;
            strlinkcommand_label = LinkCommand_Name.Replace("Command", "Label");
            //strlinkcommand_Image = LinkCommandName.Replace("Commande", "Image");
        }
        else
        {
            strlinkcommand_num = LinkCommand_Name + Convert.ToString(Index, System.Globalization.CultureInfo.InvariantCulture);
            strlinkcommand_label = LinkCommand_Name.Replace("Command", "Label") + Convert.ToString(Index, System.Globalization.CultureInfo.InvariantCulture);
        }

        if (REG_UserCVsettings != null)
        {
            REG_UserCVsettings.DeleteValue(strlinkcommand_num, false);
            REG_UserCVsettings.DeleteValue(strlinkcommand_label, false);
            // Console.WriteLine("Key delete");
        }
    }
    //---------------------------------------------------------------------------------------------------------

    public bool HasException(bool Report = false)
    {
        if (string.IsNullOrEmpty(Exception))
            return false;
        if (Report == true)
            MessageBox.Show(Exception, "Exception:", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return true;
    }
}

public class LinkCommand
{
    public int LinkIndex { get; set; }
    public string LinkName { get; set; }
    public string LinkFile { get; set; }

    public LinkCommand()
    {
        lst = new List<LinkCommand>();
    }
    public List<LinkCommand> lst { get; set; }
}

//public class LstVersion : IEquatable<LstVersion>
//{
//    public string Version_Name { get; set; }
//    public string Version_Index { get; set; }

//}
