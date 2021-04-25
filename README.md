# ScrapeIt
Utility to download content from a Weebly blog or a given Weebly page

## Inspiration
My wife hosts here blog on Weebly (http://tyannsheldonrouw.weebly.com). I suggested that she download all of her posts to have a backup. In looking at Weebly, I could only find one option to get a zip file of an archive of current pages, which didn't include blog posts.

After some searching I decided to create a utility to download the blog entry content and images.

### How it works / Notes
- Files are saved to C:\Temp\WebContent folder by default (you can change this)
- You can download a specific page (via option 1)
- You can download all "archive" pages for the blog (via option 2). To use this option, provide the archive URL (e.g. http://tyannsheldonrouw.weebly.com/blog/archives/), a start year, and an end year
- Both options download the content and images. Content is saved to an HTML file and images are saved to the "uploads" subfolder
