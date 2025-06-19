from mcp.server.fastmcp import FastMCP, Context
from typing import Dict, Any
from unity_connection import get_unity_connection


def register_capture_view_tools(mcp: FastMCP):
    """Register the capture_view tool with the MCP server."""

    @mcp.tool()
    def capture_view(ctx: Context, target: str = "game") -> Dict[str, Any]:
        """Capture a screenshot of the specified Unity view.

        Args:
            ctx: The MCP context.
            target: View to capture ('game' or 'scene').

        Returns:
            Dictionary with success status, message and data containing the
            base64 encoded PNG image.
        """
        params = {"action": "capture", "target": target}
        try:
            response = get_unity_connection().send_command("capture_view", params)
            if response.get("success"):
                return {
                    "success": True,
                    "message": response.get("message", "Captured view."),
                    "data": response.get("data"),
                }
            else:
                return {
                    "success": False,
                    "message": response.get("error", "Unknown error capturing view."),
                }
        except Exception as e:
            return {"success": False, "message": f"Python error capturing view: {str(e)}"}
