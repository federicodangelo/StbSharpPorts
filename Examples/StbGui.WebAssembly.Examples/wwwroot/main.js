// Set up the .NET WebAssembly runtime
import { dotnet } from './_framework/dotnet.js';

// Get exported methods from the .NET assembly
const { getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .create();

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

globalThis.exports = exports;

const main = exports.Main;

await main.Init();

//let result = main.RenderText("Hello Fede :-)");
//console.log(result);
//result = result.replaceAll("\n", "<br/>");
// Display the result of the .NET method
//document.getElementById("out").innerHTML = result;

function render() {
    const canvas = document.getElementById("myCanvas");
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    main.Render();
    requestAnimationFrame(render);
}

requestAnimationFrame(render);