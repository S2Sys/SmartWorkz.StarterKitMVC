"""Reports API endpoint client."""

from typing import List, Optional
from smartworkz.models import Report
from smartworkz.exceptions import (
    SmartWorkzError,
    SmartWorkzNotFoundError,
    SmartWorkzValidationError,
)


class ReportsEndpoint:
    """
    Client for Reports API endpoint.

    Provides access to:
    - List reports: GET /api/reports
    - Get report: GET /api/reports/{id}
    - Generate report: POST /api/reports
    - Get report status: GET /api/reports/{id}/status
    """

    def __init__(self, http_client):
        """Initialize ReportsEndpoint with HTTP client."""
        self._client = http_client

    def list(self, page_size: int = 50, after: Optional[str] = None) -> List[Report]:
        """
        List all reports.

        Args:
            page_size: Number of reports per page (default: 50)
            after: Cursor for pagination

        Returns:
            List of Report objects
        """
        params = {"pageSize": page_size}
        if after:
            params["after"] = after

        try:
            response = self._client.get("/api/reports", params=params)
            response.raise_for_status()
            data = response.json()

            items = data.get("data", data) if isinstance(data, dict) else data
            return [
                Report.from_dict(r) if isinstance(r, dict) else r for r in items
            ]
        except Exception as e:
            raise SmartWorkzError(f"Failed to list reports: {str(e)}")

    def get(self, report_id: str) -> Report:
        """
        Get report by ID.

        Args:
            report_id: Report ID

        Returns:
            Report object

        Raises:
            SmartWorkzNotFoundError: If report not found
        """
        if not report_id:
            raise SmartWorkzValidationError("report_id cannot be empty")

        try:
            response = self._client.get(f"/api/reports/{report_id}")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Report {report_id} not found")
            response.raise_for_status()
            return Report.from_dict(response.json())
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to get report {report_id}: {str(e)}")

    def generate(
        self,
        title: str,
        report_type: str,
        **kwargs,
    ) -> Report:
        """
        Generate new report.

        Args:
            title: Report title
            report_type: Type of report (e.g., 'sales', 'analytics', 'summary')
            **kwargs: Additional report parameters

        Returns:
            Generated Report object
        """
        if not title or not report_type:
            raise SmartWorkzValidationError("title and report_type are required")

        payload = {
            "title": title,
            "reportType": report_type,
            **kwargs,
        }

        try:
            response = self._client.post("/api/reports", json=payload)
            response.raise_for_status()
            return Report.from_dict(response.json())
        except Exception as e:
            raise SmartWorkzError(f"Failed to generate report: {str(e)}")

    def get_status(self, report_id: str) -> str:
        """
        Get report generation status.

        Args:
            report_id: Report ID

        Returns:
            Status string ('ready', 'pending', 'failed')
        """
        if not report_id:
            raise SmartWorkzValidationError("report_id cannot be empty")

        try:
            response = self._client.get(f"/api/reports/{report_id}/status")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Report {report_id} not found")
            response.raise_for_status()
            data = response.json()
            return data.get("status", "unknown")
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to get report status {report_id}: {str(e)}")

    def download(self, report_id: str) -> bytes:
        """
        Download report file.

        Args:
            report_id: Report ID

        Returns:
            Report file content as bytes

        Raises:
            SmartWorkzNotFoundError: If report not found
        """
        if not report_id:
            raise SmartWorkzValidationError("report_id cannot be empty")

        try:
            response = self._client.get(f"/api/reports/{report_id}/download")
            if response.status_code == 404:
                raise SmartWorkzNotFoundError(f"Report {report_id} not found")
            response.raise_for_status()
            return response.content
        except SmartWorkzNotFoundError:
            raise
        except Exception as e:
            raise SmartWorkzError(f"Failed to download report {report_id}: {str(e)}")
