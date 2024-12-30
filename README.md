# Crypto Ticker

Crypto Ticker is a simple desktop app built with C# that displays real-time cryptocurrency prices and their 24-hour change. It pulls data from the CoinGecko API and updates the displayed prices at regular intervals. The app features a dynamic UI and an option to stay on top of other windows.

## Features
- Displays real-time cryptocurrency prices and 24-hour change.
- Customizable tokens via a `config.json` file.
- Option to stay always on top of other windows.
- Error handling with fallback placeholder data.
- Dynamic window size based on the number of tokens.

## Screenshot
![Crypto Ticker Screenshot](https://i.imgur.com/1x6Uixs.png)

## Configuration
Edit the `config.json` file to specify which cryptocurrencies to track. Default tokens are "bitcoin", "ethereum", and "solana".

```json
{
  "Tokens": ["bitcoin", "ethereum", "solana"]
}
```

## How It Works
- The app reads `config.json` to load the tokens.
- Displays price and 24-hour change for each token.
- Updates prices every minute.
- Handles errors by showing placeholder data.
- Supports "Always on Top" mode to keep the app visible.

## Dependencies
- **Newtonsoft.Json** for JSON handling.
- **Windows Forms** for the user interface.
