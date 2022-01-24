#include <HTTPRequest.hpp>
#include <iostream>
#include <Windows.h>
#include <string>
#include <fcntl.h>
#include <io.h>
#include <vector>
#include <sstream>
#include <discord-sdk/discord.h>
#include "Resources.h"
#include <thread>
#include <chrono>
#include <codecvt>
#include <CommCtrl.h>

discord::Core* core{};
std::atomic<bool> shouldClose = false;

void ReplaceAll(std::string& string, const std::string& search, const std::string& replace) {
	size_t pos = string.find(search);
	while (pos != std::string::npos) {
		string.replace(pos, search.size(), replace);
		pos = string.find(search, pos+replace.size());
	}
}

std::string GetAlbumImageURL(const std::string& artist, const std::string& album) {
	std::string requestURL = "http://api.deezer.com/search/album?q=artist:\"" + artist + "\" album:\"" + album + "\"";
	ReplaceAll(requestURL, " ", "%20");
	http::Request request(requestURL);
	const auto response = request.send("GET");
	std::string coverUrl{ response.body.begin(), response.body.end() };

	if (response.status != http::Response::Status::Ok) {
		std::cerr << coverUrl << std::endl;
	}

	int index = coverUrl.find("cover") + 8;
	coverUrl = coverUrl.substr(index);
	index = coverUrl.find("\"");
	coverUrl = coverUrl.substr(0, index);

	ReplaceAll(coverUrl, "\\", "");
	return coverUrl;
}

void SetDiscordPlaying(const std::string& title, const std::string& album, const std::string& artist) {
	std::string details = artist + " - " + title;
	std::string albumCover = GetAlbumImageURL(artist, album);

	discord::Activity activity{};
	activity.SetDetails(details.c_str());
	activity.SetState(album.c_str());
	activity.GetAssets().SetSmallImage("original_zune_logo");
	activity.GetAssets().SetLargeImage(albumCover.c_str());

	core->ActivityManager().UpdateActivity(activity, [](discord::Result result) {
		if (result != discord::Result::Ok) {
			std::cerr << "Failed to update Discord Activity" << std::endl;
		}
	});
}

void SetDiscordStopped() {
	core->ActivityManager().ClearActivity([](discord::Result result) {
		if (result != discord::Result::Ok) {
			std::cerr << "Failed to update Discord Activity" << std::endl;
		}
	});
}

LRESULT CALLBACK WindowProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam) {
	switch (uMsg)
	{
	case WM_COPYDATA:  
		{
		COPYDATASTRUCT* data = (COPYDATASTRUCT*)lParam;
		std::wstring wideZuneMsg((wchar_t*)data->lpData, (size_t)data->cbData/2);
		std::string zuneMsg;

		std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
		zuneMsg = conv.to_bytes(wideZuneMsg);

		//Only look for messages comming from Zune
		if (zuneMsg.substr(0, 4) != "ZUNE") {
			return 0;
		}

		if (zuneMsg.length() == 22) {
			//Not Playing
			std::cout << "Not Playing" << std::endl;
			SetDiscordStopped();
		}
		else {
			//Playing
			std::stringstream tmp(zuneMsg);
			std::string segment;
			int index = 0;
		
			std::string artist;
			std::string title;
			std::string album;

			while (std::getline(tmp, segment, '\\')) {
				if (index == 4) {
					title = segment.erase(0, 1);
				}
				else if (index == 5) {
					artist = segment.erase(0, 1);
				}
				else if (index == 6) {
					album = segment.erase(0, 1);
				}

				index++;
			}	


			std::cout << "Artist: " << artist << std::endl;
			std::cout << "Title: " << title << std::endl;
			std::cout << "Album: " << album << std::endl;
			SetDiscordPlaying(title, album, artist);
		}
		}
		break;
	default:
		return DefWindowProc(hWnd, uMsg, wParam, lParam);
		break;
	}

	return 0;
}

void DiscordTick() {
	using namespace std::chrono_literals;
	while (!shouldClose) {
		core->RunCallbacks();
		std::this_thread::sleep_for(100ms);
	}
}

int main() {
	//Create Hidden Window to receive messges from Zune
	WNDCLASS wc = {};
	wc.lpszClassName = MSN_CLASS_NAME;
	wc.hInstance = HINST_THISCOMPONENT;
	wc.lpfnWndProc = WindowProc;
	//wc.hbrBackground = CreateSolidBrush(RGB(54, 57, 63));
	RegisterClass(&wc);

	HWND hWnd = CreateWindow(MSN_CLASS_NAME, L"", WS_DISABLED, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, NULL, NULL, HINST_THISCOMPONENT, NULL);

	if (!hWnd) {
		std::cerr << "Error: Could not create window!" << std::endl;
		return -1;
	}
	std::cout << "Created MSN Fake Window!" << std::endl;

	//Init Discord
	auto result = discord::Core::Create(DS_CLIENT_ID, DiscordCreateFlags_Default, &core);
	if (result != discord::Result::Ok) {
		std::cerr << "Could not initialize discord rpc" << std::endl;
		return -1;
	}
	std::thread discordTickThread(DiscordTick);
	std::wcout << "Initialized Discord RPC!" << std::endl;


	UpdateWindow(hWnd);
	MSG msg = {};
	while (GetMessage(&msg, NULL, 0, 0) > 0) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	std::cout << "Shutting down..." << std::endl;
	shouldClose = true;
	discordTickThread.join();

	return 0;
}
