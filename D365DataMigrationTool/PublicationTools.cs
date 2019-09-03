using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace D365DataMigrationTool
{
  public static class PublicationTools
  {
    /// <summary>
    /// Export The Dynamics configuration entities to a file
    /// </summary>
    /// <param name="crmUri">Uri of the Dynamics.</param>
    /// <param name="crmUserName">UserName to connect to Dynamics.</param>
    /// <param name="crmPwd">Password to connect to Dynamics.</param>
    /// <param name="entitiesLogicalNamesToExport">List of type of entities to export. "Entity1;Entity2,Guid1,Guid2;Entity3".</param>
    /// <param name="AttributesExcludedFromExport">Attributes to exclude form export. "Attribute1;Attribute2".</param>
    /// <param name="exportedFile">Path of the exported file.</param>
    public static void ExportConfigurationToFile(string connectionString, string entitiesLogicalNamesToExport, string AttributesExcludedFromExport, string exportedFile)
    {
      Console.WriteLine("Entering ExportConfigurationToFile. Export configuration to file {0}", exportedFile);
      var service = GetOrganizationService(connectionString);
      List<Entity> entities = new List<Entity>();
      List<string> entityLogicalNames = entitiesLogicalNamesToExport.Split(';').ToList();
      foreach (string entity in entityLogicalNames)
      {
        // le format de la string entity est : "Entity,Guid1,Guid2". On commence par séparer le nom de l'entité de la liste de Guid associée.
        List<string> entityIds = entity.Split(',').ToList();
        string logicalName = entityIds.ElementAt(0);
        entityIds.RemoveAt(0);
        entities.AddRange(RetrieveEntities(service, logicalName, entityIds));
      }
      Console.WriteLine("{0} entities exported", entities.Count);

      //Remove excluded attributes
      List<string> excludedAttributes = AttributesExcludedFromExport.Split(';').ToList();
      List<Entity> entitiesToExport = new List<Entity>();
      foreach (Entity entity in entities)
      {
        entitiesToExport.Add(RemoveAttributes(excludedAttributes, entity));
      }

      DataContractSerializeToXmlFile(entitiesToExport, exportedFile);
      Console.WriteLine("Ending ExportConfigurationToFile");
    }

    /// <summary>
    /// Import configuration entities to Dynamics
    /// </summary>
    /// <param name="crmUri">Uri of the Dynamics.</param>
    /// <param name="crmUserName">UserName to connect to Dynamics.</param>
    /// <param name="crmPwd">Password to connect to Dynamics.</param>
    /// <param name="entitiesLogicalNamesToImport">List of types of entities to Import. "Entity1;Entity2,Guid1,Guid2;Entity3".</param>
    /// <param name="attributesExcludedFromImport">Attributes to exclude from Import. "Attribute1;Attribute2".</param>
    /// <param name="entitiesExcludedFromUpdate">Entities to exclude from update. New Entities will be created. "Entity1;Entity2".</param>
    /// <param name="fromFile">Path of the file containing the entities to import.</param>
    public static void ImportEntitiesToCrm(string connectionString, string entitiesLogicalNamesToImport, string attributesExcludedFromImport, string entitiesExcludedFromUpdate, string fromFile)
    {
      Console.WriteLine("Entering ImportEntitiesToCrm. Import configuration from {0}", fromFile);

      var service = GetOrganizationService(connectionString);
      List<Entity> exportedEntities = new List<Entity>();
      exportedEntities = (List<Entity>)DataContractDeserialize(File.ReadAllText(fromFile), exportedEntities.GetType());

      List<Entity> entitiesToImport = new List<Entity>();

      List<Entity> existingEntities = new List<Entity>();
      List<string> entityLogicalNames = entitiesLogicalNamesToImport.Split(';').ToList();
      foreach (string entity in entityLogicalNames)
      {
        // le format de la string entity est : "Entity,Guid1,Guid2". On commence par séparer le nom de l'entité de la liste de Guid associée.
        List<string> entityIds = entity.Split(',').ToList();
        string logicalName = entityIds.ElementAt(0);
        entityIds.RemoveAt(0);

        entitiesToImport.AddRange(exportedEntities.Where(x => x.LogicalName == logicalName &&
                                                              (entityIds.Count == 0 ||
                                                               entityIds.Select(y => new Guid(y)).Contains(x.Id))));
        existingEntities.AddRange(RetrieveEntities(service, logicalName, entityIds));
      }

      List<string> excludedAttributes = attributesExcludedFromImport.Split(';').ToList();
      List<string> excludedEntitiesFromUpdate = entitiesExcludedFromUpdate.Split(';').ToList();

      bool importFailed = false;
      foreach (Entity entityToImport in entitiesToImport)
      {
        try
        {
          Entity existingEntity = existingEntities.Find(x => x.Id == entityToImport.Id);
          if (existingEntity == null)
          {
            service.Create(entityToImport);
            Console.WriteLine("Creation of {0} with Id {1}", entityToImport.LogicalName, entityToImport.Id);
          }
          else if (!excludedEntitiesFromUpdate.Contains(existingEntity.LogicalName))
          {
            Entity UpdatedEntityToImport = RemoveAttributes(excludedAttributes, entityToImport);

            service.Update(UpdatedEntityToImport);
            Console.WriteLine("Update of {0} with Id {1}", UpdatedEntityToImport.LogicalName, UpdatedEntityToImport.Id);
          }
          else
          {
            Console.WriteLine("No update needed for {0} with Id {1}", entityToImport.LogicalName, entityToImport.Id);
          }
        }
        catch (Exception e)
        {
          importFailed = true;
          AddWarning(e);
          Console.WriteLine($"Import of entity {entityToImport.LogicalName} failed:");
          Console.WriteLine(e.StackTrace);
        }
      }
      Console.WriteLine("Ending ImportEntitiesToCrm");
      if (importFailed)
      {
        throw new Exception("Not all entities could be imported. See warnings and logs for details.");
      }
    }

    private static List<Entity> RetrieveEntities(IOrganizationService service, string logicalName, List<string> entityIds)
    {
      QueryExpression qe = new QueryExpression(logicalName) { NoLock = true };
      qe.ColumnSet = new ColumnSet(true);

      if (entityIds != null && entityIds.Count > 0)
      {
        ConditionExpression cond = new ConditionExpression();
        cond.AttributeName = logicalName + "id";
        cond.Operator = ConditionOperator.In;
        foreach (string id in entityIds)
        {
          cond.Values.Add(id);
        }
        qe.Criteria.AddCondition(cond);
      }

      return service.RetrieveMultiple(qe).Entities.ToList();
    }

    public static IOrganizationService GetOrganizationService(string connectionString)
    {
      CrmServiceClient conn = new CrmServiceClient(connectionString);

      var service = (IOrganizationService)conn.OrganizationWebProxyClient != null ? (IOrganizationService)conn.OrganizationWebProxyClient : (IOrganizationService)conn.OrganizationServiceProxy;

      return service;
    }

    private static Entity RemoveAttributes(List<string> excludedAttributes, Entity entity)
    {
      Entity modifiedEntity = new Entity(entity.LogicalName, entity.Id)
      {
        RowVersion = entity.RowVersion,
      };

      foreach (string attribute in entity.Attributes.Keys)
      {
        if (!excludedAttributes.Contains(attribute))
        {
          modifiedEntity.Attributes.Add(attribute, entity.Attributes[attribute]);
        }
      }

      return modifiedEntity;
    }

    private static void DataContractSerializeToXmlFile(object obj, string file)
    {
      DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
      var settings = new XmlWriterSettings()
      {
        Indent = true,
        IndentChars = "\t"
      };
      using (var writer = XmlWriter.Create(file, settings))
      {
        serializer.WriteObject(writer, obj);
      }
    }

    private static object DataContractDeserialize(string xml, Type toType)
    {
      using (Stream stream = new MemoryStream())
      {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
        stream.Write(data, 0, data.Length);
        stream.Position = 0;
        DataContractSerializer deserializer = new DataContractSerializer(toType);
        return deserializer.ReadObject(stream);
      }
    }

    public static void AddWarning(Exception e)
    {
      Console.WriteLine($"##vso[task.logissue type=warning;sourcepath={e.Source};]{e.Message}");
    }

    public static void AddError(Exception e)
    {
      Console.WriteLine($"##vso[task.logissue type=error;sourcepath={e.Source};]{e.Message}");
    }

  }
}
