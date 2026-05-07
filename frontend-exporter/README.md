# Frontend Exporter

將 Umbraco 前台已發佈頁面匯出成可部署的靜態包（不含後台）。

## 1) 安裝

```bash
cd frontend-exporter
copy .env.example .env
npm install
```

## 2) 啟動 Umbraco（內容來源）

```bash
cd ../UmbracoSite
dotnet run --urls "http://localhost:5190"
```

## 3) 匯出前台包

```bash
cd ../frontend-exporter
npm run export
```

完成後會輸出：

- `dist/index.html`
- `dist/assets/bank-site.css`
- `dist/site-data.json`
- `bank-website.zip`

## 備註

- `.env` 可設定：
  - `UMBRACO_BASE_URL`
  - `UMBRACO_CONTENT_PATH`（預設空字串，代表根目錄 `/`）
  - `SOURCE_PAGE_URL`（可直接指定完整來源頁面 URL）
  - `PACKAGE_NAME`
- `CSS_FILE_NAME`
- `JSON_FILE_NAME`
- `EXPORT_JSON_URL`（不填則預設呼叫 Umbraco 後台 API 產 JSON）
- 此版本先輸出單頁（`/home`）做 MVP，並另外輸出 `site-data.json` 供前台 JS 使用。後續可擴成多頁批次匯出。
