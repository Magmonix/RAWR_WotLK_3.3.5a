# RAWR_WotLK_3.3.5a
Updated the RAWR code to pull items from a WotLK database instead of the post-CATA WoWHead database.
This inadvertently allows for support for green items on character load which would break the application previously.
Some items are not in the local database at all and have to be manually loaded into the application.
For the most part, when this happens, it's as simple as loading the item via its ID.
