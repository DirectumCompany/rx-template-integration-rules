using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.Integration.Structures.ImportRuleEmployee
{
  partial class Employee
  {
    public string ExternalId {get; set;}
    public string LastName {get; set;}
    public string FirstName {get; set;}
    public string MiddleName {get; set;}
    public string LoginName {get; set;}
    public string DepartmentExternalId {get; set;}
    public string JobTitleExternalId {get; set;}
    public string Email {get; set;}
    public Sungero.Core.Enumeration Status {get; set;}
  }
}