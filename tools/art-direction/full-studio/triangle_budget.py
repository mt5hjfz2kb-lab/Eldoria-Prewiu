"""Real Blender triangle delta contract. Never equate polygon faces to triangles."""
def check_triangle_growth(baseline,after,relative_allowance=0.20,min_allowance=2000):
    if type(baseline) is not int or type(after) is not int or baseline<1 or after<1:
        raise ValueError("Blender before and after evaluated triangles are required")
    budget=baseline+max(min_allowance,int(baseline*relative_allowance))
    return {"baseline_evaluated_triangles":baseline,"evaluated_triangles":after,
            "triangle_limit":budget,"triangle_delta":after-baseline,
            "allowed":after<=budget}
