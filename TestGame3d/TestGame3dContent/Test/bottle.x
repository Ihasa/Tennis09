xof 0302txt 0064
template Header {
 <3D82AB43-62DA-11cf-AB39-0020AF71E433>
 WORD major;
 WORD minor;
 DWORD flags;
}

template Vector {
 <3D82AB5E-62DA-11cf-AB39-0020AF71E433>
 FLOAT x;
 FLOAT y;
 FLOAT z;
}

template Coords2d {
 <F6F23F44-7686-11cf-8F52-0040333594A3>
 FLOAT u;
 FLOAT v;
}

template Matrix4x4 {
 <F6F23F45-7686-11cf-8F52-0040333594A3>
 array FLOAT matrix[16];
}

template ColorRGBA {
 <35FF44E0-6C7C-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
 FLOAT alpha;
}

template ColorRGB {
 <D3E16E81-7835-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
}

template IndexedColor {
 <1630B820-7842-11cf-8F52-0040333594A3>
 DWORD index;
 ColorRGBA indexColor;
}

template Boolean {
 <4885AE61-78E8-11cf-8F52-0040333594A3>
 WORD truefalse;
}

template Boolean2d {
 <4885AE63-78E8-11cf-8F52-0040333594A3>
 Boolean u;
 Boolean v;
}

template MaterialWrap {
 <4885AE60-78E8-11cf-8F52-0040333594A3>
 Boolean u;
 Boolean v;
}

template TextureFilename {
 <A42790E1-7810-11cf-8F52-0040333594A3>
 STRING filename;
}

template Material {
 <3D82AB4D-62DA-11cf-AB39-0020AF71E433>
 ColorRGBA faceColor;
 FLOAT power;
 ColorRGB specularColor;
 ColorRGB emissiveColor;
 [...]
}

template MeshFace {
 <3D82AB5F-62DA-11cf-AB39-0020AF71E433>
 DWORD nFaceVertexIndices;
 array DWORD faceVertexIndices[nFaceVertexIndices];
}

template MeshFaceWraps {
 <4885AE62-78E8-11cf-8F52-0040333594A3>
 DWORD nFaceWrapValues;
 Boolean2d faceWrapValues;
}

template MeshTextureCoords {
 <F6F23F40-7686-11cf-8F52-0040333594A3>
 DWORD nTextureCoords;
 array Coords2d textureCoords[nTextureCoords];
}

template MeshMaterialList {
 <F6F23F42-7686-11cf-8F52-0040333594A3>
 DWORD nMaterials;
 DWORD nFaceIndexes;
 array DWORD faceIndexes[nFaceIndexes];
 [Material]
}

template MeshNormals {
 <F6F23F43-7686-11cf-8F52-0040333594A3>
 DWORD nNormals;
 array Vector normals[nNormals];
 DWORD nFaceNormals;
 array MeshFace faceNormals[nFaceNormals];
}

template MeshVertexColors {
 <1630B821-7842-11cf-8F52-0040333594A3>
 DWORD nVertexColors;
 array IndexedColor vertexColors[nVertexColors];
}

template Mesh {
 <3D82AB44-62DA-11cf-AB39-0020AF71E433>
 DWORD nVertices;
 array Vector vertices[nVertices];
 DWORD nFaces;
 array MeshFace faces[nFaces];
 [...]
}

Header{
1;
0;
1;
}

Mesh {
 61;
 0.00000;0.20806;-0.15028;,
 0.10626;0.20806;-0.10626;,
 0.10626;-0.23321;-0.10626;,
 0.00000;-0.23321;-0.15028;,
 0.15028;0.20806;0.00000;,
 0.15028;-0.23321;0.00000;,
 0.10626;0.20806;0.10626;,
 0.10626;-0.23321;0.10626;,
 -0.00000;0.20806;0.15028;,
 -0.00000;-0.23321;0.15028;,
 -0.10626;0.20806;0.10626;,
 -0.10626;-0.23321;0.10626;,
 -0.15028;0.20806;-0.00000;,
 -0.15028;-0.23321;-0.00000;,
 -0.10626;0.20806;-0.10626;,
 -0.10626;-0.23321;-0.10626;,
 0.00000;0.20806;-0.15028;,
 0.00000;-0.23321;-0.15028;,
 0.00000;-0.23321;-0.00000;,
 0.00000;-0.23321;-0.00000;,
 0.00000;-0.23321;-0.00000;,
 0.00000;-0.23321;-0.00000;,
 0.00000;-0.23321;-0.00000;,
 0.00000;-0.23321;-0.00000;,
 0.00000;-0.23321;-0.00000;,
 0.00000;-0.23321;-0.00000;,
 0.00000;0.25573;-0.15028;,
 0.10626;0.25573;-0.10626;,
 0.15028;0.25573;0.00000;,
 0.10626;0.25573;0.10626;,
 -0.00000;0.25573;0.15028;,
 -0.10626;0.25573;0.10626;,
 -0.15028;0.25573;-0.00000;,
 -0.10626;0.25573;-0.10626;,
 0.00000;0.25573;-0.15028;,
 0.00000;0.50653;-0.04171;,
 0.02949;0.50653;-0.02949;,
 0.04171;0.50653;0.00000;,
 0.02949;0.50653;0.02949;,
 -0.00000;0.50653;0.04171;,
 -0.02949;0.50653;0.02949;,
 -0.04171;0.50653;-0.00000;,
 -0.02949;0.50653;-0.02949;,
 0.00000;0.50653;-0.04171;,
 0.00000;0.57884;-0.04171;,
 0.02949;0.57884;-0.02949;,
 0.00000;0.57884;0.00000;,
 0.04171;0.57884;0.00000;,
 0.00000;0.57884;0.00000;,
 0.02949;0.57884;0.02949;,
 0.00000;0.57884;0.00000;,
 -0.00000;0.57884;0.04171;,
 0.00000;0.57884;0.00000;,
 -0.02949;0.57884;0.02949;,
 0.00000;0.57884;0.00000;,
 -0.04171;0.57884;-0.00000;,
 0.00000;0.57884;0.00000;,
 -0.02949;0.57884;-0.02949;,
 0.00000;0.57884;0.00000;,
 0.00000;0.57884;-0.04171;,
 0.00000;0.57884;0.00000;;
 
 48;
 4;0,1,2,3;,
 4;1,4,5,2;,
 4;4,6,7,5;,
 4;6,8,9,7;,
 4;8,10,11,9;,
 4;10,12,13,11;,
 4;12,14,15,13;,
 4;14,16,17,15;,
 3;18,3,2;,
 3;19,2,5;,
 3;20,5,7;,
 3;21,7,9;,
 3;22,9,11;,
 3;23,11,13;,
 3;24,13,15;,
 3;25,15,17;,
 4;26,27,1,0;,
 4;27,28,4,1;,
 4;28,29,6,4;,
 4;29,30,8,6;,
 4;30,31,10,8;,
 4;31,32,12,10;,
 4;32,33,14,12;,
 4;33,34,16,14;,
 4;35,36,27,26;,
 4;36,37,28,27;,
 4;37,38,29,28;,
 4;38,39,30,29;,
 4;39,40,31,30;,
 4;40,41,32,31;,
 4;41,42,33,32;,
 4;42,43,34,33;,
 4;44,45,36,35;,
 3;46,45,44;,
 4;45,47,37,36;,
 3;48,47,45;,
 4;47,49,38,37;,
 3;50,49,47;,
 4;49,51,39,38;,
 3;52,51,49;,
 4;51,53,40,39;,
 3;54,53,51;,
 4;53,55,41,40;,
 3;56,55,53;,
 4;55,57,42,41;,
 3;58,57,55;,
 4;57,59,43,42;,
 3;60,59,57;;
 
 MeshMaterialList {
  1;
  48;
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0,
  0;;
  Material {
   0.138039;0.762353;0.800000;1.000000;;
   5.000000;
   0.000000;0.000000;0.000000;;
   0.000000;0.000000;0.000000;;
  }
 }
 MeshNormals {
  34;
  0.000000;0.000000;-1.000000;,
  0.707107;0.000000;-0.707107;,
  1.000000;0.000000;0.000000;,
  0.707107;0.000000;0.707107;,
  -0.000000;0.000000;1.000000;,
  -0.707107;0.000000;0.707107;,
  -1.000000;0.000000;0.000000;,
  -0.707107;0.000000;-0.707107;,
  1.000000;0.000000;0.000000;,
  -0.000000;0.000000;1.000000;,
  -0.707107;0.000000;0.707107;,
  0.000000;-1.000000;-0.000000;,
  0.000000;0.204039;-0.978963;,
  0.692231;0.204039;-0.692231;,
  0.978963;0.204039;0.000000;,
  0.692231;0.204039;0.692231;,
  -0.000000;0.204039;0.978963;,
  -0.692231;0.204039;0.692231;,
  -0.978963;0.204039;0.000000;,
  -0.692231;0.204039;-0.692231;,
  0.000000;0.204039;-0.978963;,
  0.692231;0.204039;-0.692231;,
  0.978963;0.204039;0.000000;,
  0.692231;0.204039;0.692231;,
  -0.000000;0.204039;0.978963;,
  -0.978963;0.204039;0.000000;,
  -0.692231;0.204039;-0.692231;,
  0.000000;1.000000;0.000000;,
  0.707107;0.000000;-0.707107;,
  1.000000;0.000000;0.000000;,
  0.707107;0.000000;0.707107;,
  -0.000000;0.000000;1.000000;,
  -1.000000;0.000000;0.000000;,
  -0.707107;0.000000;-0.707107;;
  48;
  4;0,1,1,0;,
  4;1,2,8,1;,
  4;2,3,3,8;,
  4;3,4,9,3;,
  4;4,5,10,9;,
  4;5,6,6,10;,
  4;6,7,7,6;,
  4;7,0,0,7;,
  3;11,11,11;,
  3;11,11,11;,
  3;11,11,11;,
  3;11,11,11;,
  3;11,11,11;,
  3;11,11,11;,
  3;11,11,11;,
  3;11,11,11;,
  4;12,13,1,0;,
  4;13,14,2,1;,
  4;14,15,3,2;,
  4;15,16,4,3;,
  4;16,17,5,4;,
  4;17,18,6,5;,
  4;18,19,7,6;,
  4;19,12,0,7;,
  4;20,21,13,12;,
  4;21,22,14,13;,
  4;22,23,15,14;,
  4;23,24,16,15;,
  4;24,17,17,16;,
  4;17,25,18,17;,
  4;25,26,19,18;,
  4;26,20,12,19;,
  4;0,28,21,20;,
  3;27,27,27;,
  4;28,29,22,21;,
  3;27,27,27;,
  4;29,30,23,22;,
  3;27,27,27;,
  4;30,31,24,23;,
  3;27,27,27;,
  4;31,10,17,24;,
  3;27,27,27;,
  4;10,32,25,17;,
  3;27,27,27;,
  4;32,33,26,25;,
  3;27,27,27;,
  4;33,0,20,26;,
  3;27,27,27;;
 }
 MeshTextureCoords {
  61;
  0.000000;0.000000;,
  0.125000;0.000000;,
  0.125000;1.000000;,
  0.000000;1.000000;,
  0.250000;0.000000;,
  0.250000;1.000000;,
  0.375000;0.000000;,
  0.375000;1.000000;,
  0.500000;0.000000;,
  0.500000;1.000000;,
  0.625000;0.000000;,
  0.625000;1.000000;,
  0.750000;0.000000;,
  0.750000;1.000000;,
  0.875000;0.000000;,
  0.875000;1.000000;,
  1.000000;0.000000;,
  1.000000;1.000000;,
  0.062500;1.000000;,
  0.187500;1.000000;,
  0.312500;1.000000;,
  0.437500;1.000000;,
  0.562500;1.000000;,
  0.687500;1.000000;,
  0.812500;1.000000;,
  0.937500;1.000000;,
  0.000000;0.000000;,
  0.125000;0.000000;,
  0.250000;0.000000;,
  0.375000;0.000000;,
  0.500000;0.000000;,
  0.625000;0.000000;,
  0.750000;0.000000;,
  0.875000;0.000000;,
  1.000000;0.000000;,
  0.000000;0.000000;,
  0.125000;0.000000;,
  0.250000;0.000000;,
  0.375000;0.000000;,
  0.500000;0.000000;,
  0.625000;0.000000;,
  0.750000;0.000000;,
  0.875000;0.000000;,
  1.000000;0.000000;,
  0.000000;0.000000;,
  0.125000;0.000000;,
  0.062500;0.000000;,
  0.250000;0.000000;,
  0.187500;0.000000;,
  0.375000;0.000000;,
  0.312500;0.000000;,
  0.500000;0.000000;,
  0.437500;0.000000;,
  0.625000;0.000000;,
  0.562500;0.000000;,
  0.750000;0.000000;,
  0.687500;0.000000;,
  0.875000;0.000000;,
  0.812500;0.000000;,
  1.000000;0.000000;,
  0.937500;0.000000;;
 }
}