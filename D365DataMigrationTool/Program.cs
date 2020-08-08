using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D365DataMigrationTool
{
  class Program
  {
    #region Arguments
    private const string argCommandExport = "/export";
    private const string argCommandImport = "/import";

    private const string argConnectionString = "/connectionstring";
    private const string argEntities = "/entities";
    private const string argAttributesExcluded = "/attributesexcluded";
    private const string argEntitiesOnlyCreate = "/entitiesonlycreate";
    private const string argFile = "/file";
    #endregion

    /// <summary>
    /// Export or import configuration
    /// </summary>
    /// <param name="command">"export" or "import"</param>
    /// <param name="connectionstring">connectionstring for the CDS instance.</param>
    /// <param name="entities">List of type of entities to export or import. "Entity1;Entity2,Guid1,Guid2;Entity3"</param>
    /// <param name="attributesExcluded">Attributes to exclude. "Attribute1;Attribute2"</param>
    /// <param name="file">Path of the exported file or file to import.</param>
    /// <param name="entities¸OnlyCreate">For import command, list of entities excluded for Update, only Create can be done on those entities."Entity1;Entity2"</param>
    static void Main(string[] args)
    {
      string command = "";
      string connectionString = "";
      string entities = "";
      string attributesExcluded = "";
      string file = "";
      string entitiesOnlyCreate = "";
      
      try
      {
        foreach (string arg in args)
        {
          string[] argSplited = arg.Split(":".ToCharArray(), 2);

          switch (argSplited[0])
          {
            case argCommandExport:
              command = argCommandExport;
              break;
            case argCommandImport:
              command = argCommandImport;
              break;
            case argConnectionString:
              connectionString = argSplited[1];
              break;
            case argEntities:
              entities = argSplited[1];
              break;
            case argAttributesExcluded:
              attributesExcluded = argSplited[1];
              break;
            case argEntitiesOnlyCreate:
              entitiesOnlyCreate = argSplited[1];
              break;
            case argFile:
              file = argSplited[1];
              break;
            default:
              throw new ArgumentException($"Invalid argument : {arg}");
          }
        }

        if (command == "" || connectionString == "")
        {
          throw new ArgumentException("missing argument");
        }

        switch (command)
        {
          case argCommandExport:
            PublicationTools.ExportConfigurationToFile(connectionString, entities, attributesExcluded, file);
            break;
          case argCommandImport:
            PublicationTools.ImportEntitiesToCrm(connectionString, entities, attributesExcluded, entitiesOnlyCreate, file);
            break;
          default:
            break;
        }
      }
      catch(ArgumentException e)
      {
        PublicationTools.AddError(e);
        Console.WriteLine($"Usage : D365DataMigrationTool.exe [{argCommandExport}|{argCommandImport}] {argConnectionString}:DynamicsConnectionString {argFile}:pathForOutputOrInputFile {argEntities}:entity1;entity2,guid1,guid2;entity3 {argAttributesExcluded}:attribute1;attribute2 {argEntitiesOnlyCreate}:entity1;entity2");
        throw;
      }
    }
  }
}
