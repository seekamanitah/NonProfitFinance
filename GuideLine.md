# Development Plan for Nonprofit Financial App

## Project Goal
Build a financial management app tailored for 501(c)(3) nonprofits in Tennessee, similar to a fire department. Focus on tracking income/expenses with categories, generating customizable reports with charts, integrating bank APIs for auto-sync, and supporting nonprofit-specific compliance like fund accounting, donor/grant tracking, Form 990 exports, and TN filings reminders. Primary WebUI with Android app plan. Use Visual Studio 2026, .NET ecosystem. Ensure secure, user-friendly design for admins/volunteers managing budgets, donations, and operations.

## Scope and Features
### Core Functionality
- **Income/Expense Tracking**: Add/edit/delete transactions with date, amount, description, category/subcategory, fund type (restricted/unrestricted), donor/grant ID, payee. Support splits (one transaction across multiple categories), recurring items (auto-post), transfers (income/expense pair), multi-currency with exchange rates, tags (e.g., business/emergency), rule-based auto-categorization, photo receipt upload with OCR for category guess, voice entry for mobile.
- **Categories/Subcategories**: Hierarchical tree structure (unlimited levels). Separate income and expense trees. CRUD operations: create (name, description, color/icon, budget limit), edit, delete (cascade/prevent if transactions linked, or archive), reorder/move via drag-drop. Validation: no duplicates at same level.
- **Default Categories**:
  - **Income**: Contributions (Individual/small business, Corporate, Legacies/bequests, Donated goods/services, In-kind gifts) – 25–45%; Grants (Federal, State, Local government, Foundation/trust, Nonprofit org grants) – 20–40%; Government Contracts/Fees (Local contracts, Fire protection services, Emergency response fees) – 15–35%; Fundraising Events (Special events revenue, Bingo/raffles, Non-gift event income) – 5–20%; Investment/Other Income (Interest/dividends, Asset sales, Insurance recoveries, Misc revenue) – 0–10%. Subcategory breakdowns as per examples.
  - **Expenses**: Personnel (Salaries/wages, Benefits/pensions, Payroll taxes, Volunteer reimbursements, Training stipends) – 40–60%; Equipment/Supplies (Fire gear/PPE, Vehicles/apparatus, Medical supplies, Fuel, Office supplies) – 15–30%; Maintenance/Repairs (Vehicle maintenance, Building repairs, Equipment servicing) – 8–15%; Facilities/Utilities (Rent/mortgage, Utilities, Insurance, Property taxes) – 5–12%; Training/Education (Courses/certifications, Conferences, Staff development) – 3–10%; Administrative (Accounting/legal fees, Office expenses, Telecom/postage, Advertising) – 5–12%; Program/Operations (Emergency response costs, Community outreach, Contract services) – 5–15%; Fundraising (Event costs, Donor management, Marketing) – 3–10%. Subcategory breakdowns as per examples. Percentages are benchmarks; adjustable.
- **Reports**: Monthly/yearly/semi-annual/on-demand. Filter by categories/full/subcategories, period, fund, donor/grant. Customizable layouts (compact/detailed/infographic). Export to PDF (multiple themes: modern/classic/dark/light), Excel, CSV. Include presets (save favorites), shareable read-only links. Show summaries, detailed tables, and charts: bar (comparisons/top categories), stacked bar (breakdowns), line (trends/budget vs actual), pie/donut (distributions), area (cumulatives), heatmap (spending by month), sunburst (hierarchical). Charts interactive (hover/click drill-down), filterable, themed to match report.
- **Nonprofit-Specific**: Fund accounting (track restricted/unrestricted funds). Donor/grant tracking (lists, pledges, histories, summaries). Form 990 data exports with line mappings. Revenue monitoring for $1M audit threshold. Compliance reminders for TN Secretary of State filings.
- **Import/Export**: Bidirectional Excel/CSV. Export: transactions/reports/categories/donors/grants. Import: upload with column mapping (date/amount/description/category/sub/fund/donor/grant), preview, validation (formats/numerics/matches/duplicates), auto-create missing categories. Templates with headers/instructions/nonprofit fields (Grant ID, Donor name, Restricted flag, Form 990 hints). Handle splits/transfers.
- **Bank Integration**: Auto-pull/sync transactions/balances via aggregators (Plaid, Envestnet Yodlee, Finicity, MX, Akoya) or bank-specific APIs (e.g., Citi/Capital One). Use SDKs/OAuth/tokens for secure access. Schedule syncs, encrypt data. Categorize fetched items automatically.
- **Additional**: Envelope budgeting, cash-flow forecasts (based on recurring/averages), payee auto-complete/history, dashboard widgets (burn rate/net worth/top categories), offline-first mobile with sync, end-to-end encryption.

### Tech Stack
- **Backend**: ASP.NET Core (.NET 8+) for API. Entity Framework Core for ORM. Database: SQL Server or SQLite (self-referencing tables for categories). Authentication: ASP.NET Identity.
- **Frontend (WebUI)**: Blazor WebAssembly or Razor Pages for interactive UI. Use Chart.js or Syncfusion for charts. CSS for themes.
- **Mobile (Android Plan)**: .NET MAUI for cross-platform, sharing code with web. API sync for data.
- **Reports/Exports**: Syncfusion or EPPlus for PDF/Excel/CSV generation with themes/templates.
- **Deployment**: Host on Azure/IIS. Ensure scalability, security (encryption/OAuth).

## UI/UX Design
- **Style**: Clean, professional, responsive (mobile/desktop). Palette: red-black-gray (fire dept theme). High-contrast dark/light modes. Large fonts, subtle shadows on cards/tables.
- **Layout (Desktop)**:
  - Left sidebar (collapsible): Dashboard, Transactions, Categories, Reports, Donors/Grants, Budgets, Settings.
  - Top bar: Org name/logo, quick-add button, user profile/logout, search.
  - Main area: Full-width cards/tables.
- **Dashboard**: Top: Metric cards (Month Net, YTD Net, Cash, Restricted %). Middle: Line chart (trends), pie (top expenses). Bottom: Recent transactions table, upcoming recurrings.
- **Transactions**: Filter bar (date/category/fund/donor/search). Table: Date/Desc/Category/Amount/Type (color-coded)/Fund/Actions. Inline edit, bulk actions.
- **Categories**: Tree view (expand/collapse), tabs for Income/Expense. Drag-drop, context menus (add/edit/delete/archive), color/icon picker.
- **Reports**: Period/type dropdowns, category/fund selectors, layout/theme chooser. Sections: Summary, Charts (interactive), Table. Export buttons, presets.
- **Donors/Grants**: Tabbed lists (Donors/Grants/Pledges). Card views with history links.
- **Mobile**: Bottom nav: Home/Add/Transactions/Reports/More. Hamburger for sidebar. Swipe gestures for actions.
- **Principles**: Minimize clicks for daily tasks. Intuitive for non-tech users. Accessibility: ARIA labels, keyboard nav.

## Development Phases
### Phase 1: Planning/Setup (1-2 weeks)
- Define requirements from this plan. Create wireframes/mockups (designer: Figma/Sketch). Set up VS 2026 solution: Web API project, EF Core models (Income, Expense, Category with ParentId, Transaction, Donor, Grant, Fund).
- Database schema: Tables for transactions (with splits as child records), categories (hierarchical), users, audits.

### Phase 2: Backend Development (3-4 weeks)
- Implement API endpoints: CRUD for transactions/categories/donors/grants. Query logic for reports (LINQ filters by period/category/fund).
- Integrate bank APIs: Register keys, handle OAuth, fetch/sync transactions, auto-categorize.
- Add import/export logic: Parse/map Excel/CSV, validate, commit to DB. Generate reports/exports with libraries.
- Nonprofit logic: Fund restrictions, Form 990 mappings, TN reminders (scheduled notifications), audit checks.

### Phase 3: Frontend Development (4-6 weeks)
- Build Blazor components: Forms for add/edit, tree views for categories, tables with filters/sorting.
- Dashboard: Cards/charts using libraries.
- Reports: Dynamic generation with charts (interactive), theme selectors, export triggers.
- Mobile plan: Stub MAUI project, ensure API compatibility for future sync.

### Phase 4: Integration/Testing (2-3 weeks)
- Connect frontend to backend APIs. Test end-to-end: transactions sync, reports generation, imports/exports.
- Security: Encryption, auth, input validation.
- Unit/integration tests: CRUD, queries, exports. User testing for UI flow.
- Performance: Optimize queries for large datasets.

### Phase 5: Deployment/Iteration (1-2 weeks)
- Deploy web to hosting. Plan Android build/release.
- Monitor for issues, add user feedback loop for refinements (e.g., more charts/themes).

## Risks/Considerations
- Compliance: Ensure data privacy (GDPR/HIPAA if medical), TN nonprofit laws.
- Scalability: Handle growing transactions/donors.
- Budget: Free tiers for APIs/libraries where possible.
- Team: Designer focuses on visuals/wireframes; Developer on logic/integration. Collaborate via Git/PRs.