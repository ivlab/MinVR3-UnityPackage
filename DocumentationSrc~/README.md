# Generating Web Documentation for the Package with DocFx

[DocFx](https://dotnet.github.io/docfx/index.html) is a tool for auto-generating web-based documentation for C# projects by analyzing the C# source, including xml comments.  DocFx generates a static website, which is easily hosted using github pages.  You can see [the documentation for this project on github pages here](https://pages.github.umn.edu/ivlab-cs/IVLab-Template-UnityPackage/api/IVLab.Template.html).


## Installing DocFx

Install a recent stable release of DocFx by following [the instructions on their "getting started" page](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html).  The instructions differ a bit depending upon your platform.


## Generating Documentation

Before modifying this template project to fit your needs, try running docfx on the existing project.

### Windows:
Tested with DocFX v2.57.2 on Windows
```
cd root-of-repo
docfx.exe DocFx/docfx.json --serve
```
(You may need to replace `docfx.exe` with the absolute path `C:\Absolute\Path\To\docfx.exe`)

### OSX with an M1 chip
Tested with DocFx v2.59.3 installed via homebrew following [instructions for M1 architecture on this page](https://dotnet.github.io/docfx/tutorial/docfx_getting_started.html)
```
cd DocFX
arch -x86_64 docfx docfx.json --serve  
```


## Viewing the Documentation
Then go to a browser at http://localhost:8080 to view the docs.


## Modifying the Documentation to Fit Your Project

### Configuring DocFx
- Scan through `DocFx/docfx.json` and edit the URLs and titles to fit your project.  Of special note:
  - In the global configs section, `_gitUrlPattern` should be set to "github.umn.edu" if your project is hosted on our internal github site and just "github" if your project is hosted on a public github.com site.
- You may also want to modify `DocFx\filterConfig.yml`.  You can use this to include or exclude portions of the API from the generated documentation.

### The "Improve the Doc" and "View Source" links
- DocFx tries to work with github-hosted projects to provide two special links on the right hand side of each page, one for "Improve this Doc" and one for "View Source".
- Unfortunately, the "View Source" link will only work if your project is posted publicly on github.com.  The best we can do for projects on github.umn.edu is include a link to the project's repo in the footer.  (The footer contents are configured in docfx.json.)  The "Improve this Doc" link will work for github.umn.edu but only when the `_gitUrlPattern` in `DocFx/docfx.json` is set as described above and when using the custom template we have created in the `DocFx/templates` directory.
- The "Improve this Doc" link uses what DocFx calls an overwrite file.  This is a markdown file but with a bit of special meta data at the top to tell DocFx what the contents refer to.  If you click the "Improve this Doc" link while viewing a namespace or class, DocFx will send you to the git repo and start the process of creating an override document that allows you to add extra information into the docs for the particular class or namespace you are viewing.  This is an override in the sense that you are not modifying the actual C# source file, you are adding documentation that gets associated with and replaces some pieces of the documentation generated from the C# source file.  This provides one quick way to edit the documentation.  Of course, the other is to go right to the source, which you should be able to do with the "View Source" link (if it works) or just by clicking the link to the github repo displayed in the page footer and then navigating through the repo to get to the correct file.
 If you are viewing a page that is not related to a specific class or namespace, the "Improve this Doc" link will send you to a page where you can create a new markdown file to add to the documentation.

### Adding a Landing Page, Concepts Pages, and an API-by-Theme Page
- Edit `DocFx/index.md` to create the landing page for your project.
- Edit the files inside `DocFx\concepts` to document the key concepts that are part of your project.  This is the place to include documentation that is not tied directly to a particular class or function.  You can include images and diagrams by placing them in the `DocFx\resources` folder and linking to them in your markdown file.  You can also reference classes and namespaces within the code from these "conceptual" pages.  Look at the examples in the directory to get started.
- Edit the `DocFx/api/api_by_theme.md` file to generate the API by Theme page.  DocFx will automatically generate a list of all of the namespaces, classes, etc. in your project that shows up under the "Complete API" page.  Such a list is rarely very user friendly because it is just arranged alphabetically and mixes the big classes that everyone should know about together with smaller helper classes that can distract the reader from finding what they need in your documentation.  Soooo, the purpose of this page is to provide your own curated list of the most important classes in your code, organized by theme or functionality rather than alphabetically.  DocFx cannot create this automatically for you; you have to do it manually and update it when your code changes significantly, but your users will thank you :)


## Deploying Docs on GitHub Pages

Normally we do not commit files that are easily regenerated from source to github, but GitHub Pages looks for web documentation in a special folder called `docs`. The method of deploying to GitHub pages is simply to update the contents of the `docs` folder.  DocFx is already configured to send its output directly there.  Depending on how much activity your repo has seen since the last time the docs were refreshed, the new set of docs could change dramatically, with a lot of updates, new files, and even some deleted files (e.g., if you change the name of a class).  These files should all be managed with the usual `git add`, `git rm`, `git commit`, and `git push` commands.  There is nothing special about them in that sense; just be careful because committing auto-built files like this to github can lead to many changes at once!

**Note: After you generate, the docs, *before* you commit them, make sure to
enter the Unity editor at least once so that the corresponding .meta files are
generated and your end users don't end up with hundreds of errors about missing
.meta files!**

**Note 2:** DocFX generates the documentation in place, which leaves a bunch of "obj" folders throughout your repo.  These are temporary.  You should not commit these to github!  Make sure your .gitignore ignores these obj folders.

**Note 3:** The only file in the `DocFx/api` folder that should be committed to github is `api_by_theme.md`.  All of the others are regenerated by DocFx from the source.


1. After building the docs and focusing the Unity editor, commit the generated changes
2. In your repo on github.umn.edu, go to *Settings > GitHub Pages > Source* and change it to "master branch /docs folder"
3. After a few minutes, visit https://pages.github.umn.edu/ivlab-cs/YourPackage-UnityPackage/api/IVLab.YourPackage.html
