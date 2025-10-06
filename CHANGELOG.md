# Changelog

All notable changes to this project are documented here.

## [v0.3.0] - 2025-10-06
### Added
- Inclusive date range: createdTo now includes the entire selected day.
- Validators for sort fields/orders and date ranges.

### Changed
- Custom status order: Published at the top, then Draft, then Manual approval, then others.
- Inside status groups sort by createdAt desc (newer first).

### Fixed
- Secondary sorting preserves primary ordering.

## [v0.2.0] - 2025-10-06
### Added
- Unified filter DTO (status, titleContains, createdFrom/createdTo, primary/secondary sort, pagination).
- PagedResult and DocumentDto responses.

### Changed
- Clean controller with MediatR query.

## [v0.1.0] - 2025-10-06
### Added
- Initial solution structure, Swagger, .gitignore.
