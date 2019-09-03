"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const tl = require("azure-pipelines-task-lib/task");
function run() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const connectionString = tl.getInput('ConnectionString', true);
            const entities = tl.getInput('Entities', false);
            const attributesExcluded = tl.getInput('AttributesExcluded', false);
            const entitiesOnlyCreate = tl.getInput('EntitiesOnlyCreate', false);
            const file = tl.getInput('File', true);
            var argAttributesExcluded = "";
            if (attributesExcluded != null) {
                argAttributesExcluded = `/attributesexcluded:${attributesExcluded}`;
            }
            var argEntitiesOnlyCreate = "";
            if (entitiesOnlyCreate != null) {
                argEntitiesOnlyCreate = `/entities:${entitiesOnlyCreate}`;
            }
            var path = __dirname;
            console.log(`Calling ${path}\\D365DataMigrationTool.exe /import /connectionstring:"${connectionString}" /entities:${entities} ${argAttributesExcluded} ${argEntitiesOnlyCreate} /file:${file}`);
            const { exec } = require('child_process');
            exec(`${path}\\D365DataMigrationTool.exe /import /connectionstring:"${connectionString}" /entities:${entities} ${argAttributesExcluded} ${argEntitiesOnlyCreate} /file:${file}`, (error, stdout, stderr) => {
                if (error) {
                    console.error(`exec error: ${error}`);
                    tl.setResult(tl.TaskResult.Failed, error);
                    return;
                }
                console.log(`stdout: ${stdout}`);
                if (stderr != "") {
                    console.log(`stderr: ${stderr}`);
                }
            });
        }
        catch (err) {
            tl.setResult(tl.TaskResult.Failed, err.message);
        }
    });
}
run();
