// Set up the .NET WebAssembly runtime
import { dotnet } from './_framework/dotnet.js';

// Get exported methods from the .NET assembly
const { getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .create();

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

globalThis.exports = exports;

// Access JSExport methods using exports.<Namespace>.<Type>.<Method>
const main = exports.Main;

await main.Init();

let result = main.RenderText("Hello Fede :-)");

console.log(result);

result = result.replaceAll("\n", "<br/>");

// Display the result of the .NET method
document.getElementById("out").innerHTML = result;

function render() {
    main.Render();
    requestAnimationFrame(render);
}

requestAnimationFrame(render);