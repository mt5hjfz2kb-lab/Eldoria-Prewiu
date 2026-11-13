import Foundation
import Vision
import ImageIO
guard CommandLine.arguments.count == 2 else { fatalError("Expected exact screenshot path") }
let url = URL(fileURLWithPath: CommandLine.arguments[1])
guard let source = CGImageSourceCreateWithURL(url as CFURL, nil), let image = CGImageSourceCreateImageAtIndex(source, 0, nil) else { fatalError("Unreadable genuine browser screenshot") }
let request = VNRecognizeTextRequest()
request.recognitionLevel = .accurate
request.recognitionLanguages = ["es-ES", "en-US"]
request.usesLanguageCorrection = false
try VNImageRequestHandler(cgImage: image, options: [:]).perform([request])
let rows: [[String: Any]] = (request.results ?? []).compactMap { item in
    guard let candidate = item.topCandidates(1).first else { return nil }
    let b = item.boundingBox
    return ["text": candidate.string, "confidence": candidate.confidence, "x": b.midX * 390, "y": (1 - b.midY) * 844, "width": b.width * 390, "height": b.height * 844]
}
let data = try JSONSerialization.data(withJSONObject: rows, options: [.sortedKeys])
print(String(data: data, encoding: .utf8)!)
