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

- `dist/home/index.html`
- `dist/assets/marketing-demo.css`
- `marketing-frontend.zip`

## 備註

- `.env` 可設定：
  - `UMBRACO_BASE_URL`
  - `UMBRACO_CONTENT_PATH`（預設 `home`）
  - `SOURCE_PAGE_URL`（可直接指定完整來源頁面 URL）
  - `PACKAGE_NAME`
- 此版本先輸出單頁（`/home`）做 MVP。後續可擴成多頁批次匯出。
