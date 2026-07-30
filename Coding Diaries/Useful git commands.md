
- Initialize Git: `git init`
- Clone an existent Github repo in local machine: `git clone <url of the repo>`
- Add Github repo as origin for local repo: git remote add origin `<url of the repo>`
- Configure user email and name for commits:
  `git config --global user.email "your@email.xyz"`
  `git config --global user.name "your name"`
- Get remote branch to local: `git checkout <branch name>`
- Merge local branch with your current branch: `git merge <your branch>`
- Remove files or folders from the project directory:
  `git rm --cached <filename>`
  `git rm --cached -r <pathname>`
- Stage changes: `git add <. or filename or pathname>`
- Delete local branch: `git branch --delete <branch name>`
- Delete Github remote branch: `git push -d origin <branch name>`
- Get current branch's latest changes: `git pull`
- Push your changes to Github's repo: `git push origin <your branch>`
- Commit changes with a message: `git commit -m "descriptive commit message"`