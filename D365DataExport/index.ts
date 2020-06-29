import tl = require('azure-pipelines-task-lib/task');
import { exec } from 'child_process';

async function run() {
  try {
    const connectionString: string = tl.getInput('ConnectionString', true);
    const entities: string = tl.getInput('Entities', false);
    const attributesExcluded: string = tl.getInput('AttributesExcluded', false);
    const file: string = tl.getInput('File', true);

    var argAttributesExcluded: string = "";
    if (attributesExcluded != null) {
      argAttributesExcluded = `/attributesexcluded:${attributesExcluded}`;
    }
    
    const { exec } = require('child_process');
   
    var path = __dirname;
    console.log(`calling ${path}\\D365DataMigrationTool.exe /export /connectionstring:"${connectionString}" /entities:${entities} ${argAttributesExcluded} /file:${file}`);
    
    exec(`${path}\\D365DataMigrationTool.exe /export /connectionstring:"${connectionString}" /entities:${entities} ${argAttributesExcluded} /file:${file}`, ( error: any, stdout: any, stderr: any) => {
      console.log(`stdout: ${stdout}`);
      if (stderr != "") {
        console.log(`stderr: ${stderr}`);
      }
      if (error) {
        console.error(`exec error: ${error}`);
        tl.setResult(tl.TaskResult.Failed, error);
        return;
      }
    });
  }
  catch (err) {
    tl.setResult(tl.TaskResult.Failed, err.message);
  }
}

run();