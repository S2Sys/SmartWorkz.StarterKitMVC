"""Setup configuration for SmartWorkz SDK."""

from setuptools import setup, find_packages

setup(
    name="smartworkz-sdk",
    version="1.0.0",
    description="Official Python SDK for SmartWorkz APIs",
    long_description=open("README.md").read(),
    long_description_content_type="text/markdown",
    author="S2 Systems",
    author_email="sdk@smartworkz.com",
    url="https://github.com/S2Sys/SmartWorkz",
    packages=find_packages(),
    python_requires=">=3.9",
    install_requires=[
        "httpx>=0.24.0",
        "pydantic>=2.0.0",
    ],
    extras_require={
        "dev": ["pytest>=7.0.0", "pytest-asyncio>=0.21.0", "twine>=4.0.0"],
    },
    classifiers=[
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
        "Programming Language :: Python :: 3.12",
        "License :: OSI Approved :: MIT License",
        "Development Status :: 4 - Beta",
        "Intended Audience :: Developers",
        "Topic :: Software Development :: Libraries :: Python Modules",
    ],
    keywords="smartworkz api sdk python",
)
