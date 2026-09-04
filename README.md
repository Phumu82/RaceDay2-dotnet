## 7. GitHub Repository and CI/CD

RaceDay uses GitHub for version control. All required Part 1 planning documentation is stored in the `/docs` directory.

A GitHub Actions workflow (`.github/workflows/ci.yml`) automatically validates the required Part 1 repository structure whenever changes are pushed to the `main` branch, or when a pull request targeting `main` is opened.

The workflow checks that the following required files and folders are present:

* `/docs`
* `README.md`
* RaceDay ERD (`docs/RaceDay-ERD.md`)
* API Endpoint Plan (`docs/API-Endpoint-Plan.md`)
* SQL Database Script (`docs/Database-Script.sql`)

**This is a Part 1 planning-stage CI validation only.** It checks that the required documents exist in the repository — it is **not** an application build pipeline. It does not compile any code, run any tests, connect to a database, or deploy anything, because the RaceDay application itself has not been built yet.

### GitHub Actions Successful Validation

**[INSERT REAL GITHUB ACTIONS SCREENSHOT HERE]**

The screenshot above should show a completed, green (successful) run of the "Part 1 - Planning Document Validation" workflow from the Actions tab of this repository.
