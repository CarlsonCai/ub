import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import archiver from "archiver";
import dotenv from "dotenv";

dotenv.config();

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const config = {
  baseUrl: (process.env.UMBRACO_BASE_URL || "http://localhost:5190").replace(/\/+$/, ""),
  contentPath: (process.env.UMBRACO_CONTENT_PATH || "home").replace(/^\/+/, ""),
  sourcePageUrl: process.env.SOURCE_PAGE_URL,
  outputDir: process.env.OUTPUT_DIR || "dist",
  packageName: process.env.PACKAGE_NAME || "marketing-frontend.zip",
  sourceCssPath: process.env.SOURCE_CSS_PATH || "../UmbracoSite/wwwroot/marketing-demo.css"
};

async function fetchRenderedHtml() {
  const pageUrl = config.sourcePageUrl || `${config.baseUrl}/${config.contentPath}`;
  const response = await fetch(pageUrl);

  if (!response.ok) {
    throw new Error(`無法抓取前台頁面：${pageUrl} (HTTP ${response.status})`);
  }

  return response.text();
}

async function ensureDir(dirPath) {
  await fs.promises.mkdir(dirPath, { recursive: true });
}

async function zipFolder(sourceDir, zipPath) {
  await ensureDir(path.dirname(zipPath));

  return new Promise((resolve, reject) => {
    const output = fs.createWriteStream(zipPath);
    const archive = archiver("zip", { zlib: { level: 9 } });

    output.on("close", resolve);
    archive.on("error", reject);
    archive.pipe(output);
    archive.directory(sourceDir, "marketing-frontend");
    archive.finalize();
  });
}

async function main() {
  const outputRoot = path.resolve(__dirname, config.outputDir);
  const homeDir = path.join(outputRoot, config.contentPath);
  const assetsDir = path.join(outputRoot, "assets");
  const cssSource = path.resolve(__dirname, config.sourceCssPath);
  const htmlPath = path.join(homeDir, "index.html");
  const cssTarget = path.join(assetsDir, "marketing-demo.css");
  const zipPath = path.resolve(__dirname, config.packageName);

  const html = await fetchRenderedHtml();

  await ensureDir(homeDir);
  await ensureDir(assetsDir);
  const fixedHtml = html
    .replaceAll('href="/assets/marketing-demo.css"', 'href="../assets/marketing-demo.css"')
    .replaceAll('href="/marketing-demo.css"', 'href="../assets/marketing-demo.css"');
  await fs.promises.writeFile(htmlPath, fixedHtml, "utf8");
  await fs.promises.copyFile(cssSource, cssTarget);
  await zipFolder(outputRoot, zipPath);

  console.log("匯出完成");
  console.log(`- HTML: ${htmlPath}`);
  console.log(`- CSS : ${cssTarget}`);
  console.log(`- ZIP : ${zipPath}`);
}

main().catch((error) => {
  console.error("匯出失敗:", error.message);
  process.exitCode = 1;
});
