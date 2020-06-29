# Dynamics 365 Data Migration Tool
Improve your ALM processes and simplify your build & release procedures by automatically exporting and importing your data between your multiple Dynamics 365 Customer Engagement, or Common Data Service, environments with the **Dynamics 365 Data Migration Tool**. 

This task has the same objective than the [Configuration Migration tool](https://docs.microsoft.com/en-us/dynamics365/customer-engagement/admin/manage-configuration-data) provided by Microsoft for Dynamics 365, but instead of manually launching exports and imports, you can automate them in your Azure DevOps build and release procedures.   

## Benefits
More precisely, the Dynamics 365 Data Migration Tool allows you to:
- Manage your configuration data (leading to improved ALM processes)
- Automate your data export & import in Azure DevOps Server
- Easily track & unify your changes from one environment to another
- Save time in your configuration management
- Get consistency in your entity IDs between environments (ensuring that features based on these entities are working properly in all your instances)

## Compatibility
The tool has been tested on Dynamics 365 9.1.x, but should also work on some previous versions.

## Dynamics 365 Data Export
Export your data with a single action in a XML format thanks to the **Dynamics 365 Data Export** build task. Easily integrate your exported data in a version management tool such as GIT so that you can better track your changes.

### Tips when using Dynamics 365 Data Export
The Dynamics 365 Data Export build task allows you to choose the exported entities and control the attributes to be exported.
-   By default, all items from a chosen entity will be exported. You can easily choose to only export data with specific IDs.

## Dynamics 365 Data Import
Once a XML file is produced by the Dynamics 365 Data Export task, use the **Dynamics 365 Data Import**  release task to import your data into a new environment (UAT, QA, Prod or other) with a single action.

### Tips when using Dynamics 365 Data Import
The Dynamics 365 Data Import task allows you to choose & control the entities and attributes to be imported:
-   By default, all items from a chosen entity will be imported. You can easily choose to only import data with specific IDs.

When importing entities:
-   Imported entities that were not already into the targeted instance will simply be created.
-   Imported entities that were already into the targeted instance can be  **1. updated**  or  **2. not updated**, according to the choice you made while configuring the tool.

As some entities are referring to others, be careful with the import sequence. Also, it is recommended to ignore some references by controlling the attributes to be imported. More precisely, it is recommended to exclude the following attributes:
-   organizationid
-   createdby
-   ownerid
-   owninguser
-   modifiedby
-   owningbusinessunit

If an error occurs when importing an entity, the data import will continue and conclude with an error message.

## Version History
1.0.5 Fix issue with export task

1.0.4 improve logging on error

1.0.0 Initial Release

