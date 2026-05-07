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
  contentPath: (process.env.UMBRACO_CONTENT_PATH || "").replace(/^\/+/, ""),
  sourcePageUrl: process.env.SOURCE_PAGE_URL,
  outputDir: process.env.OUTPUT_DIR || "dist",
  packageName: process.env.PACKAGE_NAME || "bank-website.zip",
  sourceCssPath: process.env.SOURCE_CSS_PATH || "../UmbracoSite/wwwroot/bank-site.css",
  cssFileName: process.env.CSS_FILE_NAME || "bank-site.css",
  exportJsonUrl: process.env.EXPORT_JSON_URL,
  jsonFileName: process.env.JSON_FILE_NAME || "site-data.json"
};

async function fetchRenderedHtml() {
  const pageUrl = config.sourcePageUrl || `${config.baseUrl}/${config.contentPath}`;
  const response = await fetch(pageUrl);

  if (!response.ok) {
    throw new Error(`無法抓取前台頁面：${pageUrl} (HTTP ${response.status})`);
  }

  return response.text();
}

function computeAssetsHref(pageOutDir, assetsDir) {
  const rel = path.relative(pageOutDir, assetsDir).split(path.sep).join("/");
  return `${rel}/${config.cssFileName}`;
}

function fixCssHref(html, cssHref) {
  return html
    .replaceAll(`href="/assets/${config.cssFileName}"`, `href="${cssHref}"`)
    .replaceAll('href="/bank-site.css"', `href="${cssHref}"`)
    .replaceAll('href="/marketing-demo.css"', `href="${cssHref}"`);
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
    archive.directory(sourceDir, "bank-website");
    archive.finalize();
  });
}

async function fetchSiteJson() {
  const url = config.exportJsonUrl || `${config.baseUrl}/site-data.json`;
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`無法抓取 JSON：${url} (HTTP ${response.status})`);
  }

  return response.text();
}

async function exportOnePage(routePath, outputRoot, assetsDir) {
  const normalized = (routePath || "").replace(/^\/+/, "");
  const isRoot = normalized === "";
  const outDir = isRoot ? outputRoot : path.join(outputRoot, normalized);
  const htmlPath = isRoot ? path.join(outputRoot, "index.html") : path.join(outDir, "index.html");
  await ensureDir(outDir);

  const pageUrl = isRoot ? `${config.baseUrl}/` : `${config.baseUrl}/${normalized}`;
  const response = await fetch(pageUrl);
  if (!response.ok) {
    throw new Error(`無法抓取前台頁面：${pageUrl} (HTTP ${response.status})`);
  }
  const html = await response.text();

  const cssHref = computeAssetsHref(outDir, assetsDir);
  const fixed = fixCssHref(html, cssHref);
  await fs.promises.writeFile(htmlPath, fixed, "utf8");
  return htmlPath;
}

async function main() {
  const outputRoot = path.resolve(__dirname, config.outputDir);
  const assetsDir = path.join(outputRoot, "assets");
  const cssSource = path.resolve(__dirname, config.sourceCssPath);
  const cssTarget = path.join(assetsDir, config.cssFileName);
  const jsonPath = path.join(outputRoot, config.jsonFileName);
  const zipPath = path.resolve(__dirname, config.packageName);

  await ensureDir(assetsDir);
  const json = await fetchSiteJson();
  await fs.promises.writeFile(jsonPath, json, "utf8");
  await fs.promises.copyFile(cssSource, cssTarget);

  const data = JSON.parse(json);
  const collections = data && data.collections ? data.collections : {};
  const announcementSlugs = Array.isArray(collections.announcements) ? collections.announcements.map((x) => x.slug).filter(Boolean) : [];
  const articleSlugs = Array.isArray(collections.articles) ? collections.articles.map((x) => x.slug).filter(Boolean) : [];
  const productSlugs = Array.isArray(collections.products) ? collections.products.map((x) => x.slug).filter(Boolean) : [];
  const promotionSlugs = Array.isArray(collections.promotions) ? collections.promotions.map((x) => x.slug).filter(Boolean) : [];

  const routes = [
    config.contentPath,
    "announcements",
    ...announcementSlugs.map((s) => `announcements/${s}`),
    "articles",
    ...articleSlugs.map((s) => `articles/${s}`),
    "products",
    ...productSlugs.map((s) => `products/${s}`),
    "promotions",
    ...promotionSlugs.map((s) => `promotions/${s}`)
  ];

  const exported = [];
  for (const r of routes) {
    exported.push(await exportOnePage(r, outputRoot, assetsDir));
  }

  await zipFolder(outputRoot, zipPath);

  console.log("匯出完成");
  console.log(`- HTML: ${exported.length} pages`);
  console.log(`- CSS : ${cssTarget}`);
  console.log(`- JSON: ${jsonPath}`);
  console.log(`- ZIP : ${zipPath}`);
}

main().catch((error) => {
  console.error("匯出失敗:", error.message);
  process.exitCode = 1;
});
