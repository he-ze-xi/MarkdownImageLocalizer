﻿# Markdown Image Localizer

## 📝 Introduction

Markdown Image Localizer is a C# console application designed to batch convert online images in your markdown files to local images. 

It efficiently downloads remote images and saves them to local asset folders named after your articles (e.g., post-title.assets), then automatically converts the links in Markdown files to relative local paths.

This tool follows the image asset management conventions commonly used by blog systems like Hexo and Typora, making it extremely convenient for static blog migration and offline reading.

## 💾 Use Cases

* Export markdown articles from platforms like CSDN, YuQue, Notion, FlowUs, Obsidian, Craft, or image hosting services to local folders

* Batch migrate old blogs and completely eliminate dependency on image hosting services

* Prepare offline archives or package content into e-books

* Rescue images from blocked or failed image hosting services

## ✨ Key Features

* Batch Processing: Processes all .md and .markdown files in the specified source folder and all its subfolders

* Concurrent Downloads: Utilizes Parallel.ForEach and HttpClient for multi-threaded file processing and image downloading with high efficiency

* Cross-file Caching: Uses ConcurrentDictionary to cache downloaded images, avoiding duplicate downloads of the same URL and saving time and bandwidth

* Path Conversion: Intelligently converts original HTTP/HTTPS links to relative paths in the format: (post-title.assets/image-name.jpg)

* Structure Preservation: Maintains the exact relative directory structure from the source folder in the converted files

* Multiple Format Support: Automatically determines and saves image formats (such as .jpg, .png, .gif, .webp, etc.) through Content-Type or URL extensions

## 🚀 How to Use

### 1. Prerequisites

Install .NET 8.0 or higher version

### 2. Usage Instructions

Run the Program:

1. Clone the repository

   Open the project in Visual Studio, compile the code, and launch the console application

2. Enter Paths:

   Enter the source folder path containing your original Markdown files

   Enter the output folder path where converted files will be saved

3. Wait for Completion:

   The program will automatically process all Markdown files and display progress

4. Console Demo:

   ```c#
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

   


## 👨‍💻 Contribution Guide

We welcome code contributions and suggestions! Please follow these steps:

1. Fork this repository

2. Create your feature branch (git checkout -b feature/amazing-feature)

3. Commit your changes (git commit -m 'Add some amazing feature')

4. Push to the branch (git push origin feature/amazing-feature)

5. Open a Pull Request

## 📄 License

MIT License