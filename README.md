# RAWR_WotLK_3.3.5a
Updated the RAWR code to pull items from a WotLK database instead of the post-CATA WoWHead database.
This inadvertently allows for support for green items on character load which would break the application previously.
Some items are not in the local database at all and have to be manually loaded into the application.
For the most part, when this happens, it's as simple as loading the item via its ID.

The /Bin/Release/Data folder contains a lot of the data that the client would be reaching out to a website to download.
I've included this to make everything run more smoothly.
The application will work without it (that's the main thing I fixed) but you can tell that the beta source code probably didn't intend for you to not have it when you downloaded it from them.
