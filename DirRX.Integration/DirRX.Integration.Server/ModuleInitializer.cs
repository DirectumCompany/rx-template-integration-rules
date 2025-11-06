using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.Integration.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      GrantRightsOnDatabooks();
    }
    
    /// <summary>
    /// Выдача прав на справочники.
    /// </summary>
    public static void GrantRightsOnDatabooks()
    {
      var allUsers = Roles.AllUsers;
      if (allUsers != null)
      {
        DirRX.Integration.IntegrationSettings.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        DirRX.Integration.IntegrationSettings.AccessRights.Save();
        
        DirRX.Integration.ImportRuleDepartments.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        DirRX.Integration.ImportRuleDepartments.AccessRights.Save();
        
        DirRX.Integration.ImportRuleEmployees.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        DirRX.Integration.ImportRuleEmployees.AccessRights.Save();
        
        DirRX.Integration.ImportRuleJobTitles.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
        DirRX.Integration.ImportRuleJobTitles.AccessRights.Save();
      }
    }
    
    /// <summary>
    /// Создание записей справочников по умолчанию.
    /// </summary>
    public static void CreateDefaultIntegrationSettings()
    {
      var integrationSettingsItem = DirRX.Integration.IntegrationSettings.GetAll().FirstOrDefault();
      if (integrationSettingsItem == null)
      {
        integrationSettingsItem = DirRX.Integration.IntegrationSettings.Create();
        integrationSettingsItem.Name = DirRX.Integration.IntegrationSettings.Info.LocalizedName;
        integrationSettingsItem.Save();
      }

      var importRuleDepartmentItem = DirRX.Integration.ImportRuleDepartments.GetAll().FirstOrDefault();
      if (importRuleDepartmentItem == null)
      {
        importRuleDepartmentItem = DirRX.Integration.ImportRuleDepartments.Create();
        importRuleDepartmentItem.Name = DirRX.Integration.ImportRuleDepartments.Info.LocalizedName;
        importRuleDepartmentItem.Note = DirRX.Integration.ImportRuleDepartments.Resources.RuleNote;
        importRuleDepartmentItem.Save();
      }
      
      var importRuleEmployeeItem = DirRX.Integration.ImportRuleEmployees.GetAll().FirstOrDefault();
      if (importRuleEmployeeItem == null)
      {
        importRuleEmployeeItem = DirRX.Integration.ImportRuleEmployees.Create();
        importRuleEmployeeItem.Name = DirRX.Integration.ImportRuleEmployees.Info.LocalizedName;
        importRuleEmployeeItem.Note = DirRX.Integration.ImportRuleEmployees.Resources.RuleNote;
        importRuleEmployeeItem.Save();
      }
      
      var importRuleJobTitleItem = DirRX.Integration.ImportRuleJobTitles.GetAll().FirstOrDefault();
      if (importRuleJobTitleItem == null)
      {
        importRuleJobTitleItem = DirRX.Integration.ImportRuleJobTitles.Create();
        importRuleJobTitleItem.Name = DirRX.Integration.ImportRuleJobTitles.Info.LocalizedName;
        importRuleJobTitleItem.Note = DirRX.Integration.ImportRuleJobTitles.Resources.RuleNote;
        importRuleJobTitleItem.Save();
      }
    }
    
  }
}
