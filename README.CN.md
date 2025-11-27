# Markdown Image Localizer

## 📝 简介

**Markdown Image Localizer** 是 C# 控制台应用程序，旨在把你的markdown文件中的在线图片批量转化为本地图。

它能够批量下载这些远程图片，并将其保存到本地的**以文章名为基础的资产文件夹（例如：`post-title.assets`）**中，然后将 Markdown 文件中的链接自动转换为相对于该资产文件夹的本地路径。

该工具遵循 **Hexo/Typora** 等博客系统常用的图片资产管理约定，极大地方便了静态博客的迁移和离线阅读。

## 💾 适用场景

- 从CSDN、语雀、Notion、FlowUs、Obsidian、Craft或者一些图床 导出markdown文章到 本地文件夹
- 批量迁移旧博客，彻底摆脱图床失效
- 准备离线存档或打包成电子书
- 拯救被墙/已挂的图床图片

## ✨ 主要功能

- **批量处理：** 可处理指定源文件夹及其所有子文件夹中的所有 `.md` 和 `.markdown` 文件。
- **并发下载：** 利用 `Parallel.ForEach` 和 `HttpClient`，实现多线程并发处理文件和下载图片，效率极高。
- **跨文件缓存：** 使用 `ConcurrentDictionary` 缓存已下载的图片，避免重复下载相同 URL 的图片，节省时间和带宽。
- **路径转换：** 智能地将原始的 HTTP/HTTPS 链接替换为相对路径，格式为：`(post-title.assets/image-name.jpg)`。
- **保留结构：** 转换后的文件将精确地保持在源文件夹中的相对目录结构。
- **支持多种格式：** 自动通过 Content-Type 或 URL 扩展名确定并保存图片格式（如 `.jpg`, `.png`, `.gif`, `.webp` 等）。

## 🚀 如何使用

### 1. 先决条件

- 安装 .NET 8.0 或更高版本。

### 2. 使用方法

1. **运行程序**：

   * Clone仓库：
   * 打开Visual Studio编译项目代码并启动控制台程序

2. **输入路径**：

   - 输入包含原始 Markdown 文件的源文件夹路径
   - 输入输出文件夹路径（转换后的文件将保存到这里）

3. **等待完成**：程序会自动处理所有 Markdown 文件并显示进度

4. 控制台运行效果演示：

   ```bash
   ==================================================
      Markdown Image Localizer (Multi-threaded)
      Automatically creates xxx.assets folders (Hexo-style)
   ==================================================
   
   Enter source folder path (containing original .md files):
   > D:\TestMarkDown
   Enter output folder path (where converted files will be saved):
   > D:\TestMarkDown\Converts
   Found 3 Markdown file(s). Starting parallel processing...
   
   [  1 / 3] Completed: ./HEFrame.WpfUI _一个现代化的 开源 WPF UI库.md
   [  2 / 3] Completed: ./lnnovationHubTool，用prism+WPF编写的MVVM模式的快速上位机软件开发框架平台.md
   [  3 / 3] Completed: ./c#学习笔记.md
   
   All 3 file(s) processed successfully!
   Output directory: D:\TestMarkDown\Converts
   
   Press any key to exit...
   ```

## 👨‍💻 贡献指南

欢迎贡献代码或提出建议！请遵循以下步骤：

1. Fork 本仓库
2. 创建你的特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交你的更改 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 开启一个 Pull Request