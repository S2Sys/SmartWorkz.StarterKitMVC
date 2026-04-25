"""Report model."""

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional, Any


@dataclass
class Report:
    """
    Represents a SmartWorkz report.

    Attributes:
        id: Unique report identifier
        title: Report title
        report_type: Type of report
        created_at: Timestamp when report was created
        updated_at: Timestamp when report was last updated
        data: Report data content
        is_ready: Whether the report is ready to be downloaded
        download_url: URL to download the report
        metadata: Optional additional report metadata
    """

    id: str
    title: str
    report_type: str
    created_at: datetime
    updated_at: Optional[datetime] = None
    data: Optional[Any] = None
    is_ready: bool = False
    download_url: Optional[str] = None
    metadata: Optional[dict] = field(default_factory=dict)

    @classmethod
    def from_dict(cls, data: dict) -> "Report":
        """Create Report from dictionary."""
        if isinstance(data.get("created_at"), str):
            data["created_at"] = datetime.fromisoformat(data["created_at"])
        if isinstance(data.get("updated_at"), str):
            data["updated_at"] = datetime.fromisoformat(data["updated_at"])
        return cls(**data)

    def __str__(self) -> str:
        status = "ready" if self.is_ready else "pending"
        return f"Report({self.id}, {self.title}, {status})"

    def __repr__(self) -> str:
        return f"Report(id={self.id!r}, title={self.title!r}, type={self.report_type!r})"

    @property
    def status(self) -> str:
        """Get report status."""
        return "ready" if self.is_ready else "pending"
