## 7. GitHub Repository and CI/CD

The RaceDay project is maintained in a GitHub repository using version control and GitHub Actions for continuous integration. All required planning documentation is stored in the `/docs` directory.

The GitHub Actions workflow automatically validates the required repository structure whenever changes are pushed to the `main` branch or a pull request is created.

The workflow checks that the following required files and folders are present:

* `/docs`
* `README.md`
* RaceDay ERD
* API endpoint plan
* SQL database script

### GitHub Actions Successful Build

The following screenshot provides evidence of a successful GitHub Actions workflow execution:

**[INSERT YOUR REAL GITHUB ACTIONS GREEN-BUILD SCREENSHOT HERE]**

The successful workflow demonstrates that the repository structure and required Part 1 documentation are being validated automatically through GitHub Actions.
