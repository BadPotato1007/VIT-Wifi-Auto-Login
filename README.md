# VIT-Wifi-Auto-Login

<img width="76" height="82" alt="Screenshot 2026-07-22 122435" src="https://github.com/user-attachments/assets/5283df65-3a32-4672-9b36-b23e2a671070" />


A simple program to automatically log in to the VIT Wi-Fi without having to input your credentials every time. This tool is designed for Windows laptops and PCs.

## The Problem
If you're a student at VIT, you know the hassle of the Wi-Fi login page appearing every time you connect or wake your computer from sleep. Entering your registration number and password repeatedly is tedious and annoying.

## The Solution
This project provides a one-time setup that installs a lightweight background script on your Windows computer. This script automatically detects when you're connected to the VIT Wi-Fi but not logged in, and then silently submits your credentials in the background to grant you internet access.

## How to Use
The setup process is designed to be as simple as possible and takes less than a minute.

1.  **Open the Setup Generator:**
    *   Go to the following website: **[https://wifi-auto-login-for-vit.vercel.app/]([https://badpotato1007.github.io/vit-wifi-auto-login/](https://wifi-auto-login-for-vit.vercel.app/))**

2.  **Enter Your Credentials:**
    *   Type your **Registration Number** and **Password** into the provided fields.

3.  **Generate and Copy the Command:**
    *   Click the `Generate Setup Command` button.
    *   A long command will appear in a text box. Click the `Copy Command` button to copy it.

4.  **Run in PowerShell:**
    *   Press the `Windows Key` on your keyboard.
    *   Type **`PowerShell`** and press `Enter` to open it.
    *   Right-click (or `Ctrl+V`) inside the PowerShell window to paste the command.
    *   Press `Enter` to run the command.

5.  **Done!**
    *   A small pop-up window will appear confirming that the setup was successful. The script is now running, and it will automatically start every time you log in to your computer. Enjoy seamless Wi-Fi!

## How It Works
For those interested in the technical details:

1.  **Generator Page (`index.html`):** The webpage you visit does not send your credentials to any server. All processing happens locally in your browser using JavaScript.
2.  **Command Generation:** The page takes your credentials and embeds them into a series of scripts, which are then encoded into a single PowerShell command.
3.  **PowerShell Execution:** When you run the command in PowerShell, it performs the following actions:
    *   Creates a directory at `%APPDATA%\ProntoAutoLogin`.
    *   Creates a batch file (`ProntoAutoLogin.bat`) in this directory. This is the core script that checks your connection status and uses `curl` to post your credentials to the VIT login portal (`phc.prontonetworks.com`) if you aren't online.
    *   Creates a VBScript file (`SilentStart.vbs`) in your computer's `Startup` folder. This script's only purpose is to run the main batch file silently in the background whenever you log in.
    *   Starts the script for the first time.

## Security & Privacy
Your credentials are a sensitive matter. Here’s how this tool handles them:

*   **Local Processing:** The setup generator is a client-side webpage. Your registration number and password are never sent to any server or seen by anyone else. They are processed directly in your browser.
*   **Local Storage:** The script stores your credentials in a file (`creds.dat`) inside the `%APPDATA%\ProntoAutoLogin` folder on your own computer. They are not stored in plain text within the script itself.
*   **Open Source:** The code is completely open-source. You can review the `index.html` file to see exactly how the command is generated and what the scripts do. Your trust is paramount, and you are encouraged to check the code yourself.
