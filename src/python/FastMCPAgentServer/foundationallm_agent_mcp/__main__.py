"""Entry point for `python -m foundationallm_agent_mcp`."""

from .server import server


def main() -> None:
    server.run()


if __name__ == "__main__":
    main()

