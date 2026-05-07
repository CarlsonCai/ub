# Ub 專案操作手冊

這份文件整理此專案的日常操作流程，包含：

- 啟動 Umbraco（前台/後台）
- 後台修改內容
- 前台打包（CLI 與後台一鍵）
- Git 提交與推送
- 常見問題排除

---

## 1. 啟動專案（本地）

```powershell
cd C:\Users\sherl\Desktop\ub\UmbracoSite
dotnet run --urls "http://localhost:5191"
```

啟動後網址：

- 後台：`http://localhost:5191/umbraco`
- 前台頁：`http://localhost:5191/home`
- 公告列表：`http://localhost:5191/announcements`
- 文章列表：`http://localhost:5191/articles`

---

## 2. 後台修改行銷頁

1. 進入後台 `Content`
2. 打開 `Home`（LandingPage）
3. 修改欄位（例如 `Header Brand Text`）
4. 按 `Save and Publish`
5. 前台 `http://localhost:5191/home` 檢查結果

---

## 2.1 後台上稿（公告 / 文章）

1. 進入後台 `Content`
2. 開啟 Dashboard「上稿工具 / 匯出」
3. 在「公告 / 文章上稿」區塊新增/編輯內容
4. 點「儲存上稿內容」
5. 前台：
   - 公告：`http://localhost:5191/announcements`
   - 文章：`http://localhost:5191/articles`

---

## 3. 前台打包（CLI）

```powershell
cd C:\Users\sherl\Desktop\ub\frontend-exporter
npm run export
```

輸出結果：

- `C:\Users\sherl\Desktop\ub\frontend-exporter\dist\home\index.html`
- `C:\Users\sherl\Desktop\ub\frontend-exporter\dist\assets\bank-site.css`
- `C:\Users\sherl\Desktop\ub\frontend-exporter\dist\site-data.json`
- `C:\Users\sherl\Desktop\ub\frontend-exporter\bank-website.zip`

> ZIP 解壓後會有一層 `bank-website` 資料夾，內含 `home`、`assets` 與 `site-data.json`。

---

## 4. 後台一鍵打包

1. 進入後台 `Content` 區段
2. 開啟「上稿工具 / 匯出」Dashboard
3. 點 `開始打包`
4. 打包成功後會自動觸發下載

下載位置：

- 下載到每位使用者「瀏覽器預設下載資料夾」

---

## 5. Git 操作

### 5.1 查看狀態

```powershell
cd C:\Users\sherl\Desktop\ub
git status
```

### 5.2 提交變更

```powershell
git add .
git commit -m "your message"
```

### 5.3 推送到 GitHub

```powershell
git push
```

---

## 6. 常見問題排除

### Q1: 打包出現 `fetch failed`

原因通常是 Umbraco 沒啟動或埠號不一致。

請確認：

- Umbraco 有在跑：`http://localhost:5191`
- `frontend-exporter\.env` 內為：
  - `UMBRACO_BASE_URL=http://localhost:5191`

---

### Q2: CSS 沒吃到

請重新打包一次：

```powershell
cd C:\Users\sherl\Desktop\ub\frontend-exporter
npm run export
```

並確認 HTML 使用相對路徑：

- `../assets/bank-site.css`

---

### Q3: Port 被占用無法啟動

查詢 PID：

```powershell
netstat -ano | Select-String 5191 | Select-String LISTENING
```

關閉程序：

```powershell
taskkill /PID <PID> /F
```

---

### Q4: GitHub push 失敗（Could not resolve host / auth failed）

- 先確認網路可連 `github.com`
- 確認已登入 GitHub（CLI 或 Credential Manager）
- 再執行：

```powershell
git push -u origin main
```

---

## 7. 專案目錄重點

- `UmbracoSite/`：Umbraco 專案（前後台）
- `frontend-exporter/`：前台靜態匯出工具
- `frontend-exporter/dist/`：匯出結果
- `frontend-exporter/bank-website.zip`：部署包

